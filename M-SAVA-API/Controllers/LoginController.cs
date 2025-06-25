using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using M_SAVA_BLL.Models;
using M_SAVA_BLL.Services;

namespace M_SAVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            LoginResponseDTO success = await _loginService.LoginAsync(request);
            if (success == null)
                return Unauthorized();

            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            bool success = await _loginService.RegisterAsync(request);
            if (!success)
                return BadRequest("Registration failed.");

            return Ok();
        }
    }
}
