using MSAVA_BLL.Loggers;
using MSAVA_BLL.Services.Interfaces;
using MSAVA_DAL.Models;
using MSAVA_DAL.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MSAVA_BLL.Services.Access
{
    public class InviteCodeService
    {
        private readonly BaseDataContext _context;
        private readonly IUserService _userService;
        private readonly ServiceLogger _serviceLogger;

        public InviteCodeService(
            BaseDataContext context,
            IUserService userService,
            ServiceLogger serviceLogger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
            _context.InviteCodes.Add(inviteCode);
            await _context.SaveChangesAsync();
            _serviceLogger.WriteLog(InviteLogActions.InviteCodeCreated, $"Invite code created by user {user.Username}.", user.Id, inviteCode.Id);
            return inviteCode.Id;
        }

        public int GetHowManyUses(Guid inviteCode)
        {
            int usedCount = _context.Users.AsNoTracking().Count(u => u.InviteCodeId == inviteCode);
            return usedCount;
        }

        public int GetRemainingUses(Guid inviteCode)
        {
            int usedCount = GetHowManyUses(inviteCode);
            InviteCodeDB? inviteCodeDB = _context.InviteCodes.AsNoTracking().SingleOrDefault(ic => ic.Id == inviteCode);
            if (inviteCodeDB == null) throw new KeyNotFoundException($"Repository: Entity with id {inviteCode} not found.");
            return inviteCodeDB.MaxUses - usedCount;
        }

        public bool IsValidInviteCode(Guid inviteCodeId)
        {
            int remainingUses = GetRemainingUses(inviteCodeId);
            return remainingUses > 0;
        }

        public List<InviteCodeDB> GetAllInviteCodes()
        {
            return _context.InviteCodes.AsNoTracking().ToList();
        }

        public InviteCodeDB GetInviteCodeById(Guid inviteCodeId)
        {
            return _context.InviteCodes.AsNoTracking().SingleOrDefault(i => i.Id == inviteCodeId)
                ?? throw new KeyNotFoundException($"Repository: Entity with id {inviteCodeId} not found.");
        }
    }
}
