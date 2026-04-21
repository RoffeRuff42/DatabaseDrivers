using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using TodoApi.DTOs;
using TodoApi.Services; 

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/v1/todos")]
    [EnableRateLimiting("sliding")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _service;
        private readonly IMemoryCache _cache;

        public TodosController(ITodoService service, IMemoryCache cache)
        {
            _service = service;
            _cache = cache;

        }

        /// <summary>
        /// Hämtar alla todos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTodos(int page = 1, int pageSize = 10, string? search = null, string? ticketId = null)
        {

            var cacheKey = $"todos_{page}_{pageSize}_{search}_{ticketId}";

            if (!_cache.TryGetValue(cacheKey, out List<TodoResponseDto>? todos))
            {
                // Updated to call the Async version with the TestTicket
                todos = await _service.GetAllAsync(page, pageSize, search, ticketId);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(cacheKey, todos, cacheOptions);
            }
            return Ok(todos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodo(int id, string ticketId)
        {
            string cacheKey = $"todo_{id}_{ticketId}";

            if (!_cache.TryGetValue(cacheKey, out TodoResponseDto? todo))
            {
                // Updated to call the Async version
                todo = await _service.GetByIdAsync(id, ticketId);

                if (todo == null)
                    return NotFound();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(cacheKey, todo, cacheOptions);
            }

            return Ok(todo);
        }

        /// <summary>
        /// Skapar en ny todo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTodo(CreateTodoDto dto)
        {
            // Updated to call CreateTodoAsync
            var createdTodo = await _service.CreateTodoAsync(dto, dto.TicketId);

            return CreatedAtAction(
                nameof(GetTodo),
                new { id = createdTodo!.Id },
                createdTodo
            ); 
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, UpdateTodoDto dto)
        {
            // Updated to call UpdateTodoAsync
            var updated = await _service.UpdateTodoAsync(id, dto, dto.TicketId);

            if (!updated)
                return NotFound();

            return NoContent(); 
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id, string ticketId)
        {
            // Updated to call DeleteTodoAsync
            var deleted = await _service.DeleteTodoAsync(id, ticketId);

            if (!deleted)
                return NotFound();

            return NoContent(); 
        }
    }
}