using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserApi.DTOs;
using UserApi.Services;

namespace UserApi.Controllers
{
    /// <summary>
    /// Authentication controller. Users send credentials and receive a JWT token on successful authentication.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("sliding")]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthService _authService;

        public AuthController(IUserAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token when credentials are valid.
        /// </summary>
        /// <param name="dto">Login request DTO containing username and password.</param>
        /// <returns>200 OK with an object containing the JWT token when credentials are valid; 401 Unauthorized otherwise.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto dto) // DTO for login request
        {
            var token = _authService.Login(dto.Username, dto.Password);

            if (token is null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // Return the JWT token
            return Ok(new { Token = token });
        }
    }
}
