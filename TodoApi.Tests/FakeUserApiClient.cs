using TodoApi.Clients;
using TodoApi.DTOs;

public class FakeUserApiClient : IUserApiClient
{
    public Task<UserLoginDto?> ValidateTicketAsync(string ticketId)
    {
        return Task.FromResult<UserLoginDto?>(new UserLoginDto
        {
            UserId = 1,

            // REQUIRED 
            Username = "TestUser",

        });
    }
}