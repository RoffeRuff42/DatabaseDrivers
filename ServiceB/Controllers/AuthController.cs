using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserApi.DTOs;
using UserApi.Services;

namespace UserApi.Controllers
{
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

        // NOTE: ValidateTicket is removed because we are now using JWT tokens
        //[HttpGet("validate/{ticketId}")]
        //public IActionResult ValidateTicket(string ticketId)
        //{
        //    var internalApiKey = _configuration["UserApiConfig:InternalApiKey"];
        //    var requestApiKey = Request.Headers["x-api-key"].FirstOrDefault();

        //    if (string.IsNullOrWhiteSpace(internalApiKey) || requestApiKey != internalApiKey)
        //        return Unauthorized(new { message = "Invalid API key." });

        //    var ticket = _authService.ValidateTicket(ticketId);

        //    if (ticket is null)
        //        return NotFound(new { message = "Ogiltig ticket." });

        //    return Ok(ticket);
        //}
    }
}
