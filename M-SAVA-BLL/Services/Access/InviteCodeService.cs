using M_SAVA_BLL.Loggers;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services.Access
{
    public class InviteCodeService
    {
        private readonly IIdentifiableRepository<InviteCodeDB> _inviteCodeRepository;
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IUserService _userService;
        private readonly ServiceLogger _serviceLogger;

        public InviteCodeService(
            IIdentifiableRepository<InviteCodeDB> inviteCodeRepo,
            IIdentifiableRepository<UserDB> userRepository,
            IUserService userService,
            ServiceLogger serviceLogger)
        {
            _inviteCodeRepository = inviteCodeRepo ?? throw new ArgumentNullException(nameof(inviteCodeRepo));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public async Task<Guid> CreateNewInviteCode(int maxUses, DateTime expiresAt)
        {
            UserDB user = _userService.GetSessionUserDB();
            InviteCodeDB inviteCode = new InviteCodeDB
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                MaxUses = maxUses
            };
            _inviteCodeRepository.Insert(inviteCode);
            await _inviteCodeRepository.SaveChangesAsync();
            _serviceLogger.WriteLog(InviteLogActions.InviteCodeCreated, $"Invite code created by user {user.Username}.", user.Id, inviteCode.Id);
            return inviteCode.Id;
        }

        public int GetHowManyUses(Guid inviteCode)
        {
            int usedCount = _userRepository.GetAllAsReadOnly().Count(u => u.InviteCodeId == inviteCode);
            return usedCount;
        }

        public int GetRemainingUses(Guid inviteCode)
        {
            int usedCount = GetHowManyUses(inviteCode);
            InviteCodeDB inviteCodeDB = _inviteCodeRepository.GetById(inviteCode);
            return inviteCodeDB.MaxUses - usedCount;
        }

        public bool IsValidInviteCode(Guid inviteCodeId)
        {
            int remainingUses = GetRemainingUses(inviteCodeId);
            return remainingUses > 0;
        }

        public List<InviteCodeDB> GetAllInviteCodes()
        {
            return _inviteCodeRepository.GetAllAsReadOnly().ToList();
        }

        public async Task<InviteCodeDB?> GetInviteCodeById(Guid inviteCodeId)
        {
            return await _inviteCodeRepository.GetByIdAsync(inviteCodeId);
        }
    }
}
