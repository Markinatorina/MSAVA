using MSAVA_BLL.Loggers;
using MSAVA_BLL.Services.Interfaces;
using MSAVA_DAL.Models;
using MSAVA_DAL.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MSAVA_BLL.Services.Access
{
    public class AccessGroupService
    {
        private readonly BaseDataContext _context;
        private readonly IUserService _userService;
        private readonly ServiceLogger _serviceLogger;

        public AccessGroupService(
            BaseDataContext context,
            IUserService userService,
            ServiceLogger serviceLogger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public List<AccessGroupDB> GetUserAccessGroups(Guid userId)
        {
            // Load user with access groups included to ensure navigation is available
            var user = _context.Users
                .AsNoTracking()
                .Include(u => u.AccessGroups)
                .SingleOrDefault(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException($"Repository: Entity with id {userId} not found.");
            return user.AccessGroups?.ToList() ?? new List<AccessGroupDB>();
        }

        public Guid CreateAccessGroup(string name)
        {
            Guid userId = _userService.GetSessionUserId();
            var user = _context.Users.SingleOrDefault(u => u.Id == userId) 
                ?? throw new KeyNotFoundException($"Repository: Entity with id {userId} not found.");
            DateTime now = DateTime.UtcNow;

            AccessGroupDB accessGroup = new AccessGroupDB
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                CreatedAt = now,
                Name = name,
                Users = new List<UserDB>(),
                SubGroups = new List<AccessGroupDB>()
            };

            _context.AccessGroups.Add(accessGroup);
            _context.SaveChanges();
            _serviceLogger.WriteLog(GroupLogActions.AccessGroupCreated, $"Access group '{name}' created by user {user.Username}.", user.Id, accessGroup.Id);

            // Ensure the user's access groups collection is initialized and add the creating user to the new access group
            user.AccessGroups ??= new List<AccessGroupDB>();
            user.AccessGroups.Add(accessGroup);

            _context.Users.Update(user);
            _context.SaveChanges();

            _serviceLogger.WriteLog(GroupLogActions.AccessGroupUserAdded, $"User {user.Username} added to access group '{name}'.", user.Id, accessGroup.Id);
            return accessGroup.Id;
        }

        public async Task AddAccessGroupToUserAsync(Guid accessGroupId, Guid userId)
        {
            // Retrieve entities; throw if not found
            var accessGroup = await _context.AccessGroups
                .SingleOrDefaultAsync(g => g.Id == accessGroupId);
            if (accessGroup == null)
                throw new KeyNotFoundException($"Repository: Entity with id {accessGroupId} not found.");

            var user = await _context.Users
                .Include(u => u.AccessGroups)
                .SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException($"Repository: Entity with id {userId} not found.");

            if (!IsSessionUserAdminOrOwnerOfAccessGroup(accessGroup))
            {
                throw new UnauthorizedAccessException("Only the owner or an admin can add users to this access group.");
            }

            // Prevent duplicate membership
            if (user.AccessGroups != null && user.AccessGroups.Any(g => g.Id == accessGroup.Id))
            {
                throw new InvalidOperationException($"User with ID {user.Id} is already in AccessGroup with ID {accessGroup.Id}.");
            }

            user.AccessGroups.Add(accessGroup);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _serviceLogger.WriteLog(GroupLogActions.AccessGroupUserAdded, $"User {user.Username} added to access group '{accessGroup.Name}'.", user.Id, accessGroup.Id);
        }

        private bool IsSessionUserAdminOrOwnerOfAccessGroup(AccessGroupDB accessGroup)
        {
            bool isAdmin = _userService.IsSessionUserAdmin();
            bool isOwner = IsSessionUserOwnerOfAccessGroup(accessGroup);
            return isAdmin || isOwner;
        }

        private bool IsSessionUserOwnerOfAccessGroup(AccessGroupDB accessGroup)
        {
            Guid userId = _userService.GetSessionUserId();
            return userId == accessGroup.OwnerId;
        }
    }
}
