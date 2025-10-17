using MSAVA_Shared.Models;
using MSAVA_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSAVA_BLL.Services.Interfaces
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
