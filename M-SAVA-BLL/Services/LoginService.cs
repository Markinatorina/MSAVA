using M_SAVA_BLL.Models;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services
{
    public class LoginService
    {
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IIdentifiableRepository<JWTDB> _jwtRepository;

        // Jwt constants
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;

        // Password hashing constants
        private const int SaltSize = 32;
        private const int HashIterations = 100_000;
        private const int HashByteSize = 32;

        public LoginService(IIdentifiableRepository<UserDB> userRepository, IIdentifiableRepository<JWTDB> jwtRepository, string jwtSecret, string jwtIssuer)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtRepository = jwtRepository ?? throw new ArgumentNullException(nameof(jwtRepository));
            _jwtSecret = jwtSecret ?? throw new ArgumentNullException(nameof(jwtSecret));
            _jwtIssuer = jwtIssuer ?? throw new ArgumentNullException(nameof(jwtIssuer));
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            UserDB? user = await _userRepository.GetAllAsReadOnly()
                .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                return new LoginResponseDTO { Token = string.Empty };

            JWTDB token = await GenerateJwtTokenAsync(user);

            return new LoginResponseDTO { Token = token.TokenString };
        }

        public async Task<JWTDB> GenerateJwtTokenAsync(UserDB user)
        {
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddHours(2);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim("isAdmin", user.IsAdmin.ToString()),
                new Claim("isBanned", user.IsBanned.ToString()),
                new Claim("isWhitelisted", user.IsWhitelisted.ToString()),
                new Claim("inviteCode", user.InviteCode.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtIssuer,
                claims: claims,
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            JWTDB jwtDb = new JWTDB
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Username = user.Username,
                IsAdmin = user.IsAdmin,
                IsBanned = user.IsBanned,
                IsWhitelisted = user.IsWhitelisted,
                InviteCode = user.InviteCode,
                TokenString = tokenString,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt
            };
            _jwtRepository.Insert(jwtDb);
            await _jwtRepository.CommitAsync();

            return jwtDb;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            bool exists = await _userRepository.GetAllAsReadOnly()
                .AnyAsync(u => u.Username == request.Username, cancellationToken);
            if (exists)
                return false;

            if (request.InviteCode == Guid.Empty)
                return false;

            byte[] salt = GenerateSalt();
            byte[] hash = HashPassword(request.Password, salt);

            UserDB user = new UserDB
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _userRepository.Insert(user);
            await _userRepository.CommitAsync();

            return true;
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, HashIterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(HashByteSize);
        }

        private static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            byte[] hash = HashPassword(password, storedSalt);
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}
