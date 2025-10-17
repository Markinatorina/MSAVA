using MSAVA_Shared.Models;
using MSAVA_DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MSAVA_INF.Environment;
using MSAVA_BLL.Utils;
using MSAVA_BLL.Services.Interfaces;
using MSAVA_BLL.Loggers;
using MSAVA_DAL.Contexts;

namespace MSAVA_BLL.Services.Access
{
    public class LoginService : ILoginService
    {
        private readonly BaseDataContext _context;
        private readonly InviteCodeService _inviteCodeService;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly byte[] _jwtKeyBytes;
        private readonly ServiceLogger _serviceLogger;

        public LoginService(
            BaseDataContext context,
            InviteCodeService inviteCodeService,
            ILocalEnvironment env,
            ServiceLogger serviceLogger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jwtIssuer = env.Values.JwtIssuerName;
            _jwtAudience = env.Values.JwtIssuerAudience;
            _jwtKeyBytes = env.GetSigningKeyBytes();
            _inviteCodeService = inviteCodeService ?? throw new ArgumentNullException(nameof(inviteCodeService));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            // Load user and their access groups for claims
            UserDB? user = await _context.Users
                .AsNoTracking()
                .Include(u => u.AccessGroups)
                .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            if (user == null || !PasswordUtils.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new InvalidOperationException("Username doesn't exist or password is incorrect.");
            }

            JwtDB token = await GenerateJwtTokenAsync(user);

            _serviceLogger.WriteLog(UserLogAction.SessionLogIn, $"User {user.Username} logged in successfully.", user.Id, null);

            return new LoginResponseDTO { Token = token.TokenString };
        }

        public async Task<JwtDB> GenerateJwtTokenAsync(UserDB user)
        {
            Guid jwtId = Guid.NewGuid();
            DateTime issuedAt = DateTime.UtcNow;
            DateTime expiresAt = issuedAt.AddHours(2);

            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
            };
            
            if (user.IsAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            if (user.IsBanned)
                claims.Add(new Claim(ClaimTypes.Role, "Banned"));
            if (user.IsWhitelisted)
                claims.Add(new Claim(ClaimTypes.Role, "Whitelisted"));
            claims.Add(new Claim("inviteCode", user.InviteCodeId?.ToString() ?? string.Empty));

            List<Guid> accessGroupGuids = user.AccessGroups?.Select(g => g.Id).ToList() ?? new List<Guid>();
            claims.Add(new Claim("accessGroups", string.Join(",", accessGroupGuids)));

            SymmetricSecurityKey key = new SymmetricSecurityKey(_jwtKeyBytes);
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: creds);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            JwtDB jwtDb = new JwtDB
            {
                Id = jwtId,
                UserId = user.Id,
                Username = user.Username,
                IsAdmin = user.IsAdmin,
                IsBanned = user.IsBanned,
                IsWhitelisted = user.IsWhitelisted,
                TokenString = tokenString,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt
            };
            _context.Jwts.Add(jwtDb);
            await _context.SaveChangesAsync();

            return jwtDb;
        }

        public async Task<Guid> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Validate invite code format early
            if (request.InviteCode == Guid.Empty)
            {
                throw new InvalidOperationException("Invite code is required.");
            }

            // Validate invite code existence and remaining uses
            bool isValidInviteCode = _inviteCodeService.IsValidInviteCode(request.InviteCode);
            if (!isValidInviteCode)
            {
                throw new InvalidOperationException("Invalid or expired invite code.");
            }

            bool exists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Username == request.Username, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            byte[] salt = PasswordUtils.GenerateSalt();
            byte[] hash = PasswordUtils.HashPassword(request.Password, salt);

            UserDB user = new UserDB
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsAdmin = false,
                IsBanned = false,
                IsWhitelisted = false,
                InviteCodeId = request.InviteCode,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _serviceLogger.WriteLog(UserLogAction.AccountRegistered, $"User {user.Username} registered successfully.", user.Id, null);

            return user.Id;
        }
    }
}