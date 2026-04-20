using UserApi.DTOs;

namespace UserApi.Services
{
    public interface IUserAuthService
    {
        UserTicketDto? Login(string username, string password);
        UserTicketDto? ValidateTicket(string ticketId);
    }
}
