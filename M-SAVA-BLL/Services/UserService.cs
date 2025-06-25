using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services
{
    public class UserService
    {
        private readonly IIdentifiableRepository<UserDB> _userRepository;

        public UserService(IIdentifiableRepository<UserDB> userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        // Read
        public UserDB GetUserById(Guid id)
        {
            return _userRepository.GetById(id);
        }

        public async Task<UserDB> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }

        public IEnumerable<UserDB> GetAllUsers()
        {
            return _userRepository.GetAllAsReadOnly().ToList();
        }

        // Delete
        public void DeleteUser(Guid id)
        {
            _userRepository.DeleteById(id);
            _userRepository.Commit();
        }

        public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _userRepository.DeleteByIdAsync(id);
            await _userRepository.CommitAsync();
        }
    }
}