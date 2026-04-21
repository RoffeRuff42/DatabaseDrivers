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
        private readonly IConfiguration _configuration;

        public AuthController(IUserAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto dto)
        {
            var result = _authService.Login(dto.Username, dto.Password);

            if (result is null)
                return Unauthorized(new { message = "Fel användarnamn eller lösenord." });

            return Ok(result);
        }

        [HttpGet("validate/{ticketId}")]
        public IActionResult ValidateTicket(string ticketId)
        {
            var internalApiKey = _configuration["UserApiConfig:InternalApiKey"];
            var requestApiKey = Request.Headers["x-api-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(internalApiKey) || requestApiKey != internalApiKey)
                return Unauthorized(new { message = "Invalid API key." });

            var ticket = _authService.ValidateTicket(ticketId);

            if (ticket is null)
                return NotFound(new { message = "Ogiltig ticket." });

            return Ok(ticket);
        }
    }
}
