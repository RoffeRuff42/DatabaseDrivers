using System.Threading.Tasks;
using TodoApi.Clients;
using TodoApi.DTOs;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private readonly TodoDbContext _context; // Our new connection to the database
        private readonly IExternalApiClient _externalApiClient;

        // Note: IUserApiClient is removed because we don't need to "call home" to validate tickets anymore!

        public TodoService(TodoDbContext context, IExternalApiClient externalApiClient)
        {
            _context = context; // Assign the injected context
            _externalApiClient = externalApiClient;
        }

        public async Task<List<TodoResponseDto>> GetAllAsync(int page, int pageSize, string? search, int userId)
        {
            // We use the userId passed from the controller (extracted from JWT)
            var query = _context.Todos.Where(t => t.UserId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.ToLower().Contains(search.ToLower()));
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TodoResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsDone = t.IsDone,
                    UserId = t.UserId
                })
                .ToListAsync();
        }

        public async Task<TodoResponseDto?> GetByIdAsync(int id, int userId)
        {
            // Ensure the todo belongs to the user
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null) return null;

            return new TodoResponseDto
            {
                Id = todo.Id,
                Title = todo.Title,
                IsDone = todo.IsDone,
                UserId = todo.UserId
            };
        }

        public async Task<TodoResponseDto?> CreateTodoAsync(CreateTodoDto dto, int userId)
        {
            var randomQuote = await _externalApiClient.GetTestDataAsync("random");

            var todo = new Todo
            {
                Title = dto.Title,
                IsDone = false,
                UserId = userId // Set from the JWT userId
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            return new TodoResponseDto
            {
                Id = todo.Id,
                Title = todo.Title,
                IsDone = todo.IsDone,
                UserId = todo.UserId
            };
        }

        public async Task<bool> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto, int userId)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null) return false;

            todo.Title = updateTodoDto.Title;
            todo.IsDone = updateTodoDto.IsDone;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTodoAsync(int id, int userId)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null) return false;

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
