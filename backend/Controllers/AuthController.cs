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

        [HttpGet("me")]
        public async Task<IActionResult> Me([FromHeader(Name = "X-Session-Cookie")] string sessionCookieHeader)
        {
            var cookie = sessionCookieHeader;
            if (string.IsNullOrEmpty(cookie))
            {
                if (Request.Headers.TryGetValue("Cookie", out var cookieHeaderValue))
                    cookie = cookieHeaderValue.ToString();
            }

            if (string.IsNullOrEmpty(cookie))
            {
                return BadRequest("Session cookie is required in 'X-Session-Cookie' or 'Cookie' header.");
            }

            var result = await _blackboardService.GetUserDataAsync(cookie);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}