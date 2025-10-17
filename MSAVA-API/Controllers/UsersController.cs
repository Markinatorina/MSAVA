using MSAVA_BLL.Services.Interfaces;
using MSAVA_DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MSAVA_API.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            return Ok(_userService.GetSessionUser());
        }

        [HttpGet("claims")]
        public IActionResult GetUserClaims()
        {
            return Ok(_userService.GetSessionClaims());
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {            
            return Ok(_userService.GetAllUsers());
        }
    }
}
