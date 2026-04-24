using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using TodoApi.DTOs;
using TodoApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/v1/todos")]
    [EnableRateLimiting("sliding")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _service;
        private readonly IMemoryCache _cache;

        public TodosController(ITodoService service, IMemoryCache cache)
        {
            _service = service;
            _cache = cache;

        }

        // Helper method to extract user ID from JWT claims
        private int GetUserId() 
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Hämtar alla todos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTodos(int page = 1, int pageSize = 10, string? search = null)
        {

            int userId = GetUserId();
            var cacheKey = $"todos_{userId}_{page}_{pageSize}_{search}";

            if (!_cache.TryGetValue(cacheKey, out List<TodoResponseDto>? todos))
            {
                todos = await _service.GetAllAsync(page, pageSize, search, userId);

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
            int userId = GetUserId();
            string cacheKey = $"todo_{id}_{userId}";

            if (!_cache.TryGetValue(cacheKey, out TodoResponseDto? todo))
            {
                todo = await _service.GetByIdAsync(id, userId);

                if (todo == null)
                    return NotFound();

                _cache.Set(cacheKey, todo, TimeSpan.FromMinutes(5));
            }

            return Ok(todo);
        }

        /// <summary>
        /// Skapar en ny todo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTodo(CreateTodoDto dto)
        {
            int userId = GetUserId();
            var createdTodo = await _service.CreateTodoAsync(dto, userId);

            if (createdTodo == null) return BadRequest();

            return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, UpdateTodoDto dto)
        {
            int userId = GetUserId();
            var updated = await _service.UpdateTodoAsync(id, dto, userId);

            if (!updated)
                return NotFound();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            int userId = GetUserId();
            var deleted = await _service.DeleteTodoAsync(id, userId);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}