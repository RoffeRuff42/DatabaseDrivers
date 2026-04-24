using UserApi.DTOs;

namespace UserApi.Services
{
    public interface IUserAuthService
    {
        // Now returns a string (the JWT token) instead of a DTO
        string? Login(string username, string password);
    }
}
