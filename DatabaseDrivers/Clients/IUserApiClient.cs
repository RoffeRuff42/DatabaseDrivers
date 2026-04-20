using TodoApi.DTOs;

namespace TodoApi.Clients
{
    public interface IUserApiClient
    {
        Task<UserLoginDto?> ValidateTicketAsync(string ticketId);

    }
}
