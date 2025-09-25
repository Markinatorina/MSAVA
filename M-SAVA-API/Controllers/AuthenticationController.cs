using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using M_SAVA_Shared.Models;
using M_SAVA_BLL.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace M_SAVA_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public AuthenticationController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody][Required] LoginRequestDTO request)
        {
            var result = await _loginService.LoginAsync(request);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody][Required] RegisterRequestDTO request)
        {
            var result = await _loginService.RegisterAsync(request);
            return Ok(result);
        }
    }
}
