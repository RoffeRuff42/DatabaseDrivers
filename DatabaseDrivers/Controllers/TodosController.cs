using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using TodoApi.DTOs;
using TodoApi.Services; 

namespace TodoApi.Controllers
{
    /// <summary>
    /// Controller that manages todo items.
    /// Provides endpoints to list, retrieve, create, update and delete todos.
    /// </summary>
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
        /// Retrieves a paged list of todos.
        /// </summary>
        /// <param name="page">Page number (1-based). Defaults to 1.</param>
        /// <param name="pageSize">Number of items per page. Defaults to 10.</param>
        /// <param name="search">Optional search term to filter by title.</param>
        /// <param name="ticketId">Ticket id used to validate the user session.</param>
        /// <returns>Returns 200 OK with a list of <see cref="TodoResponseDto"/> items.</returns>
        /// <response code="200">Successfully retrieved a page of todos.</response>
        /// <response code="401">If the provided ticketId is not valid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
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

        /// <summary>
        /// Retrieves a single todo by id.
        /// </summary>
        /// <param name="id">The id of the todo to fetch.</param>
        /// <param name="ticketId">Ticket id used to validate the user session.</param>
        /// <returns>Returns 200 OK with the requested <see cref="TodoResponseDto"/>.</returns>
        /// <response code="200">Todo item found and returned.</response>
        /// <response code="404">Todo item with the specified id was not found.</response>
        /// <response code="401">If the provided ticketId is not valid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
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
        /// Creates a new todo item.
        /// </summary>
        /// <param name="dto">The create DTO containing the todo data and ticket id.</param>
        /// <returns>Returns 201 Created with the created <see cref="TodoResponseDto"/>.</returns>
        /// <response code="201">Todo was created successfully.</response>
        /// <response code="400">If the supplied DTO is invalid.</response>
        /// <response code="401">If the provided ticketId is not valid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
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

        /// <summary>
        /// Updates an existing todo item.
        /// </summary>
        /// <param name="id">The id of the todo to update.</param>
        /// <param name="dto">The update DTO containing the new values and ticket id.</param>
        /// <returns>Returns 204 No Content on success.</returns>
        /// <response code="204">Update successful.</response>
        /// <response code="400">If the supplied DTO is invalid.</response>
        /// <response code="404">If no todo with the given id exists.</response>
        /// <response code="401">If the provided ticketId is not valid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, UpdateTodoDto dto)
        {
            // Updated to call UpdateTodoAsync
            var updated = await _service.UpdateTodoAsync(id, dto, dto.TicketId);

            if (!updated)
                return NotFound();

            return NoContent(); 
        }

        /// <summary>
        /// Deletes a todo item by id.
        /// </summary>
        /// <param name="id">The id of the todo to delete.</param>
        /// <param name="ticketId">Ticket id used to validate the user session.</param>
        /// <returns>Returns 204 No Content on success.</returns>
        /// <response code="204">Delete successful.</response>
        /// <response code="404">If no todo with the given id exists.</response>
        /// <response code="401">If the provided ticketId is not valid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
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