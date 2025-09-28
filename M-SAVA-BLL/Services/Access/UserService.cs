using M_SAVA_Shared.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_BLL.Loggers;

namespace M_SAVA_BLL.Services.Access
{
    public class UserService : IUserService
    {
        private readonly IIdentifiableRepository<UserDB> _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceLogger _serviceLogger;

        public UserService(IIdentifiableRepository<UserDB> userRepository, IHttpContextAccessor httpContextAccessor, ServiceLogger serviceLogger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public UserDTO GetUserById(Guid id)
        {
            var userDb = _userRepository.GetById(id, u => u.AccessGroups);
            return MappingUtils.MapUserDTOWithRelationships(userDb);
        }

        public Guid GetSessionUserId()
        {
            // Retrieve user id from session claims stored in HttpContext
            return GetSessionDto().UserId;
        }

        public SessionDTO GetSessionClaims()
        {
            // Retrieve full session claims stored in HttpContext
            return GetSessionDto();
        }

        public bool IsSessionUserAdmin()
        {
            // Determine if the current session user has admin role
            return GetSessionDto().IsAdmin;
        }

        public UserDTO GetSessionUser()
        {
            return MappingUtils.MapUserDTOWithRelationships(GetSessionUserDB());
        }

        public UserDB GetSessionUserDB()
        {
            // Load the current session user with access groups included
            Guid sessionUserId = GetSessionUserId();
            var userDb = _userRepository.GetById(sessionUserId, u => u.AccessGroups);
            return userDb;
        }

        public List<UserDTO> GetAllUsers()
        {
            var userDbs = _userRepository.GetAllAsReadOnly().ToList();
            return userDbs.Select(MappingUtils.MapUserDTOWithRelationships).ToList();
        }

        public void DeleteUser(Guid id)
        {
            _userRepository.DeleteById(id);
            _userRepository.SaveChanges();
            _serviceLogger.WriteLog(UserLogAction.AccountDeletion, $"User deleted: {id}", id, null);
        }

        private SessionDTO GetSessionDto()
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Items["SessionDTO"] is SessionDTO sessionDto)
            {
                return sessionDto;
            }
            throw new UnauthorizedAccessException("SessionDTO not found in HttpContext.");
        }
    }
}