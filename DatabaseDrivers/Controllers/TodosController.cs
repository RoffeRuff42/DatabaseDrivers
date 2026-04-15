using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
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
        public IActionResult GetTodos(int page = 1, int pageSize = 10, string? search = null)
        {

            var cacheKey = $"todos_{page}_{pageSize}_{search}";

            if (!_cache.TryGetValue(cacheKey, out List<TodoResponseDto> todos))
            {

                todos = _service.GetAll(page, pageSize, search);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                _cache.Set(cacheKey, todos, cacheOptions);
            }
            return Ok(todos);
        }

        [HttpGet("{id}")]
        public IActionResult GetTodo(int id)
        {
            string cacheKey = $"todo_{id}";

            if (!_cache.TryGetValue(cacheKey, out var todo))
            {
                todo = _service.GetById(id);

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
        public IActionResult CreateTodo(CreateTodoDto dto)
        {
            var createdTodo = _service.Create(dto);

            return CreatedAtAction(
                nameof(GetTodo),
                new { id = createdTodo.Id },
                createdTodo
            ); 
        }

       
        [HttpPut("{id}")]
        public IActionResult UpdateTodo(int id, UpdateTodoDto dto)
        {
            var updated = _service.UpdateTodo(id, dto);

            if (!updated)
                return NotFound(); 

            return NoContent(); 
        }

       
        [HttpDelete("{id}")]
        public IActionResult DeleteTodo(int id)
        {
            var deleted = _service.DeleteTodo(id);

            if (!deleted)
                return NotFound(); 

            return NoContent(); 
        }
    }
}