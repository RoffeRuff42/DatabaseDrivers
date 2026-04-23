using Microsoft.Extensions.Caching.Memory;
using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Services
{
    public class UserAuthService : IUserAuthService
    {

        private readonly IMemoryCache _cache;

        private static readonly List<User> _users = new List<User>
        {
            new User { UserId = 1, Username = "Robin", Password = "password123" },
            new User { UserId = 2, Username = "Lisa", Password = "password123" },
            new User { UserId = 3, Username = "Liza", Password = "password123" },
            new User { UserId = 4, Username = "Rolf", Password = "password123" }
        };
        public UserAuthService(IMemoryCache cache)
        {
            _cache = cache;
        }
        public UserTicketDto? Login(string username, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (user is null)
                return null;

            var ticket = new UserTicketDto
            {
                UserId = user.UserId,
                Username = user.Username,
                TicketId = Guid.NewGuid().ToString()
            };

            _cache.Set(ticket.TicketId, ticket, TimeSpan.FromMinutes(30));

            return ticket;
        }

        public UserTicketDto? ValidateTicket(string ticketId)
        {
            _cache.TryGetValue(ticketId, out UserTicketDto? ticket);
            return ticket;
        }
    }
}
