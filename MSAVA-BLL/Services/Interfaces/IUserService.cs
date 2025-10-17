using M_SAVA_Shared.Models;
using M_SAVA_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services.Interfaces
{
    public interface IUserService
    {
        UserDTO GetUserById(Guid id);
        List<UserDTO> GetAllUsers();
        void DeleteUser(Guid id);
        bool IsSessionUserAdmin();
        UserDTO GetSessionUser();
        Guid GetSessionUserId();
        UserDB GetSessionUserDB();
        SessionDTO GetSessionClaims();
    }
}
