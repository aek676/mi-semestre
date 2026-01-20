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
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
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
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserResponseDto>> Me([FromHeader(Name = "X-Session-Cookie")] string? sessionCookieHeader)
        {
            var cookie = sessionCookieHeader;
            if (string.IsNullOrEmpty(cookie) && Request.Headers.TryGetValue("Cookie", out var cookieHeaderValue))
            {
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