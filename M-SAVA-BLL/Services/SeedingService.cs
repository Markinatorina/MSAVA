using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M_SAVA_DAL.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Repositories;
using M_SAVA_INF.Environment;
using System;
using System.Linq;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_BLL.Loggers;

namespace M_SAVA_BLL.Services
{
    public class SeedingService : ISeedingService
    {
        private readonly IIdentifiableRepository<UserDB> _userRepo;
        private readonly IIdentifiableRepository<InviteCodeDB> _inviteCodeRepo;
        private readonly ILocalEnvironment _env;
        private readonly ServiceLogger _serviceLogger;

        public SeedingService(
            IIdentifiableRepository<UserDB> userRepo,
            IIdentifiableRepository<InviteCodeDB> inviteCodeRepo,
            ILocalEnvironment env,
            ServiceLogger serviceLogger)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _inviteCodeRepo = inviteCodeRepo ?? throw new ArgumentNullException(nameof(inviteCodeRepo));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public void Seed()
        {
            SeedAdminUser();
        }

        private Guid SeedAdminUser()
        {
            string adminUsername = _env.Values.AdminUsername;
            string adminPassword = _env.Values.AdminPassword;

            UserDB? adminUser = _userRepo.GetAllAsReadOnly().FirstOrDefault(u => u.Username == adminUsername);
            if (adminUser == null)
            {
                byte[] salt = PasswordUtils.GenerateSalt();
                byte[] hash = PasswordUtils.HashPassword(adminPassword, salt);
                adminUser = new UserDB
                {
                    Id = Guid.NewGuid(),
                    Username = adminUsername,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsAdmin = true,
                    IsBanned = false,
                    IsWhitelisted = true,
                    CreatedAt = DateTime.UtcNow
                };
                _userRepo.Insert(adminUser);
                _userRepo.SaveChanges();
                _serviceLogger.WriteLog(UserLogAction.AccountCreation, $"Admin user '{adminUsername}' created during seeding.", adminUser.Id, null);
            }
            return adminUser.Id;
        }
    }
}
