using System.Threading.Tasks;
using TodoApi.Clients;
using TodoApi.DTOs;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.Extensions.Caching.Memory;

namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private readonly TodoDbContext _context; // Our new connection to the database
        private readonly IExternalApiClient _externalApiClient;
        private readonly IMemoryCache _cache;

        // Note: IUserApiClient is removed because we don't need to "call home" to validate tickets anymore!

        public TodoService(TodoDbContext context, IExternalApiClient externalApiClient, IMemoryCache cache)
        {
            _context = context; // Assign the injected context
            _externalApiClient = externalApiClient;
            _cache = cache;
        }
        private string GetTodosCacheKey(int page, int pageSize, string? search, int userId)
        {
            return $"todos_{userId}_{page}_{pageSize}_{search}";
        }

        private string GetTodoCacheKey(int id, int userId)
        {
            return $"todo_{id}_{userId}";
        }

        private string GetTodoListVersionKey(int userId)
        {
            return $"todos_version_{userId}";
        }

        private int GetTodoListVersion(int userId)
        {
            string versionKey = GetTodoListVersionKey(userId);

            if (!_cache.TryGetValue(versionKey, out int version))
            {
                version = 1;
                _cache.Set(versionKey, version);
            }

            return version;
        }

        private void InvalidateTodoListCache(int userId)
        {
            string versionKey = GetTodoListVersionKey(userId);
            int currentVersion = GetTodoListVersion(userId);

            _cache.Set(versionKey, currentVersion + 1);
        }
        public async Task<List<TodoResponseDto>> GetAllAsync(int page, int pageSize, string? search, int userId)
        {
            int version = GetTodoListVersion(userId);
            string cacheKey = $"{GetTodosCacheKey(page, pageSize, search, userId)}_v{version}";

            if (_cache.TryGetValue(cacheKey, out List<TodoResponseDto>? cachedTodos))
            {
                return cachedTodos!;
            }
            // We use the userId passed from the controller (extracted from JWT)
            var query = _context.Todos.Where(t => t.UserId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.ToLower().Contains(search.ToLower()));
            }

            var todos = await query
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

            _cache.Set(cacheKey, todos);

            return todos;
        }

        public async Task<TodoResponseDto?> GetByIdAsync(int id, int userId)
        {
            string cacheKey = GetTodoCacheKey(id, userId);

            if (_cache.TryGetValue(cacheKey, out TodoResponseDto? cachedTodo))
            {
                return cachedTodo;
            }

            // Ensure the todo belongs to the user
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null) return null;

            var todoDto = new TodoResponseDto
            {
                Id = todo.Id,
                Title = todo.Title,
                IsDone = todo.IsDone,
                UserId = todo.UserId
            };

            _cache.Set(cacheKey, todoDto);

            return todoDto;
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

            var todoDto = new TodoResponseDto
            {
                Id = todo.Id,
                Title = todo.Title,
                IsDone = todo.IsDone,
                UserId = todo.UserId
            };

            _cache.Set(GetTodoCacheKey(todo.Id, userId), todoDto);
            InvalidateTodoListCache(userId);

            return todoDto;
        }

        public async Task<bool> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto, int userId)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null) return false;

            todo.Title = updateTodoDto.Title;
            todo.IsDone = updateTodoDto.IsDone;

            await _context.SaveChangesAsync();

            _cache.Remove(GetTodoCacheKey(id, userId));
            InvalidateTodoListCache(userId);

            return true;
        }

        public async Task<bool> DeleteTodoAsync(int id, int userId)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (todo == null) return false;

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            _cache.Remove(GetTodoCacheKey(id, userId));
            InvalidateTodoListCache(userId);
            return true;
        }
    }
}
