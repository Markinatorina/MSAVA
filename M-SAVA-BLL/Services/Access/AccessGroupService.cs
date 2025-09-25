using M_SAVA_BLL.Loggers;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services.Access
{
    public class AccessGroupService
    {
        private readonly IIdentifiableRepository<AccessGroupDB> _accessGroupRepository;
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IUserService _userService;
        private readonly ServiceLogger _serviceLogger;

        public AccessGroupService(
            IIdentifiableRepository<AccessGroupDB> accessGroupRepo,
            IUserService userService,
            IIdentifiableRepository<UserDB> userRepository,
            ServiceLogger serviceLogger)
        {
            _accessGroupRepository = accessGroupRepo ?? throw new ArgumentNullException(nameof(accessGroupRepo));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public async Task<List<AccessGroupDB>> GetUserAccessGroupsAsync(Guid userId)
        {
            // Load user with access groups included to ensure navigation is available
            UserDB user = await _userRepository.GetByIdAsync(userId, u => u.AccessGroups);
            return user.AccessGroups?.ToList() ?? new List<AccessGroupDB>();
        }

        public async Task<Guid> CreateAccessGroupAsync(string name)
        {
            Guid userId = _userService.GetSessionUserId();
            UserDB user = _userRepository.GetByIdAsTracked(userId);
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

            _accessGroupRepository.Insert(accessGroup);
            await _accessGroupRepository.SaveChangesAsync();
            _serviceLogger.WriteLog(GroupLogActions.AccessGroupCreated, $"Access group '{name}' created by user {user.Username}.", user.Id, accessGroup.Id);

            // Ensure the user's access groups collection is initialized and add the creating user to the new access group
            user.AccessGroups ??= new List<AccessGroupDB>();
            user.AccessGroups.Add(accessGroup);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            _serviceLogger.WriteLog(GroupLogActions.AccessGroupUserAdded, $"User {user.Username} added to access group '{name}'.", user.Id, accessGroup.Id);
            return accessGroup.Id;
        }

        public async Task AddAccessGroupToUserAsync(Guid accessGroupId, Guid userId)
        {
            // Retrieve entities; repository throws if not found
            AccessGroupDB accessGroup = await _accessGroupRepository.GetByIdAsync(accessGroupId);
            UserDB user = await _userRepository.GetByIdAsync(userId, u => u.AccessGroups);

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

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

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
