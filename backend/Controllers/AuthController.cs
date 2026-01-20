using Microsoft.AspNetCore.Mvc;
using backend.Dtos;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IBlackboardService _blackboardService;

        public AuthController(IBlackboardService blackboardService)
        {
            _blackboardService = blackboardService;
        }

        [HttpPost("login-ual")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Usuario y contraseña requeridos.");
            }

            var result = await _blackboardService.AuthenticateAsync(request.Username, request.Password);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }
    }
}