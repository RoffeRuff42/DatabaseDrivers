using System.Threading.Tasks;
using TodoApi.Clients;
using TodoApi.DTOs;

namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private static readonly List<TodoResponseDto> _todos = new(); // In-memory list used to store all todos (acts as a temporary data store instead of a database)
        private static int _nextId = 1;  // Counter used to generate unique IDs for each todo 
        private readonly IUserApiClient _userApiClient; // Dependency on an internal user API client to fetch user information
        private readonly IExternalApiClient _externalApiClient; // Dependency on an external API client to fetch additional data

        public TodoService(IUserApiClient userApiClient, IExternalApiClient externalApiClient)
        {
            _userApiClient = userApiClient;
            _externalApiClient = externalApiClient;
        }

        public async Task<List<TodoResponseDto>> GetAllAsync(int page, int pageSize, string? search, string ticketId)
        {
            try
            {
                var userTicket = await _userApiClient.ValidateTicketAsync(ticketId);
            }
            var query = _todos.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.ToLower().Contains(search.ToLower()));
            }

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<TodoResponseDto?> GetByIdAsync(int id, string ticketId)
        {
            //Validate the ticket before fetching the todo item
            var userTicket = await _userApiClient.ValidateTicketAsync(ticketId);
            if (userTicket == null)
            {
                throw new UnauthorizedAccessException("Invalid session."); // Return invalid session error if the ticket is not valid
            }

            //Get randomquote from external API to demonstrate usage of external API client
            var randomQuote = await _externalApiClient.GetTestDataAsync("random-quote");

            return _todos.FirstOrDefault(t => t.Id == id);
        }

        public async Task<TodoResponseDto?> CreateTodoAsync(CreateTodoDto dto, string ticketId)
        {
            //Validate the ticket before fetching the todo item
            var userTicket = await _userApiClient.ValidateTicketAsync(ticketId);
            if (userTicket == null)
            {
                throw new UnauthorizedAccessException("Invalid session."); // Return invalid session error if the ticket is not valid
            }

            //Get randomquote from external API to demonstrate usage of external API client
            var randomQuote = await _externalApiClient.GetTestDataAsync("random-quote");

            var todo = new TodoResponseDto
            {
                Id = _nextId++,
                Title = dto.Title,
                IsDone = false
                UserId = userTicket.UserId // Associate the todo item with the user ID from the validated ticket
            };

            _todos.Add(todo);
            return todo;
        }
        public async Task<bool> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto, string ticketId)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return false;

            todo.Title = updateTodoDto.Title;
            todo.IsDone = updateTodoDto.IsDone;

            return true;
        }
        public async Task<bool> DeleteTodoAsync(int id, string ticketId)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return false;

            _todos.Remove(todo);
            return true;
        }
    

    }

}
