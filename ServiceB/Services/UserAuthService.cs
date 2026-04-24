using Microsoft.Extensions.Caching.Memory;
using UserApi.DTOs;
using UserApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserApi.Services
{
    public class UserAuthService : IUserAuthService
    {

        private readonly IConfiguration _configuration;

        private static readonly List<User> _users = new List<User>
        {
            new User { UserId = 1, Username = "Robin", Password = "password123" },
            new User { UserId = 2, Username = "Lisa", Password = "password123" },
            new User { UserId = 3, Username = "Liza", Password = "password123" },
            new User { UserId = 4, Username = "Rolf", Password = "password123" }
        };
        public UserAuthService(IConfiguration configuration) // Inject IConfiguration to access JWT settings
        {
            _configuration = configuration;
        }
        public string? Login(string username, string password) // Now returns a string (the JWT token) instead of a DTO
        {
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (user is null)
                return null;

            // Instead of a ticket in memory, we create a JWT
            return CreateToken(user);
        }

        private string CreateToken(User user) // This method creates a JWT token for the authenticated user
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Add claims to the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            
            // Create the JWT token with the specified claims and signing credentials
            var token = new JwtSecurityToken( 
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds // Use the signing credentials to sign the token
            );

            return new JwtSecurityTokenHandler().WriteToken(token); // Return the generated JWT token as a string
        }
    }
}
