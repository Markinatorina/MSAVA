using M_SAVA_Shared.Models;
using M_SAVA_BLL.Utils;
using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using M_SAVA_BLL.Services.Interfaces;
using M_SAVA_BLL.Loggers;
using M_SAVA_DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace M_SAVA_BLL.Services.Access
{
    public class UserService : IUserService
    {
        private readonly BaseDataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceLogger _serviceLogger;

        public UserService(BaseDataContext context, IHttpContextAccessor httpContextAccessor, ServiceLogger serviceLogger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
        }

        public UserDTO GetUserById(Guid id)
        {
            var userDb = _context.Users
                .AsNoTracking()
                .Include(u => u.AccessGroups)
                .SingleOrDefault(u => u.Id == id) 
                ?? throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
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
            var userDb = _context.Users
                .AsNoTracking()
                .Include(u => u.AccessGroups)
                .SingleOrDefault(u => u.Id == sessionUserId) 
                ?? throw new KeyNotFoundException($"Repository: Entity with id {sessionUserId} not found.");
            return userDb;
        }

        public List<UserDTO> GetAllUsers()
        {
            var userDbs = _context.Users.AsNoTracking().ToList();
            return userDbs.Select(MappingUtils.MapUserDTOWithRelationships).ToList();
        }

        public void DeleteUser(Guid id)
        {
            var user = _context.Users.SingleOrDefault(u => u.Id == id) 
                ?? throw new KeyNotFoundException($"Repository: Entity with id {id} not found.");
            _context.Users.Remove(user);
            _context.SaveChanges();
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