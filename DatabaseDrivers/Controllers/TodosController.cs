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
        public async Task<IActionResult> GetTodos(int page = 1, int pageSize = 10, string? search = null)
        {

            var cacheKey = $"todos_{page}_{pageSize}_{search}";

            if (!_cache.TryGetValue(cacheKey, out List<TodoResponseDto> todos))
            {

                todos = await _service.GetAllAsync(page, pageSize, search, "ticket123");

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(cacheKey, todos, cacheOptions);
            }
            return Ok(todos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodo(int id)
        {
            string cacheKey = $"todo_{id}";

            if (!_cache.TryGetValue(cacheKey, out TodoResponseDto? todo))
            {
                // 🔹 ÄNDRING: async metod + ticketId
                todo = await _service.GetByIdAsync(id, "ticket123");


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
            // 🔹 ÄNDRING: async + ticketId
            var createdTodo = await _service.CreateTodoAsync(dto, "ticket123");

            return CreatedAtAction(
                nameof(GetTodo),
                new { id = createdTodo.Id },
                createdTodo
            ); 
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, UpdateTodoDto dto)
        {
            // 🔹 ÄNDRING: async + ticketId
            var updated = await _service.UpdateTodoAsync(id, dto, "ticket123");


            if (!updated)
                return NotFound(); 

            return NoContent(); 
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            // 🔹 ÄNDRING: async + ticketId
            var deleted = await _service.DeleteTodoAsync(id, "ticket123");

            if (!deleted)
                return NotFound(); 

            return NoContent(); 
        }
    }
}