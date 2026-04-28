using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;
using TodoApi.DTOs;
using TodoApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Asp.Versioning;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Controller that manages todo items.
    /// Provides endpoints to list, retrieve, create, update and delete todos.
    /// Authentication and authorization are handled via JWT tokens; the user id is extracted from token claims.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/todos")]
    [ApiVersion(1.0)]
    [ApiVersion(2.0)]
    [EnableRateLimiting("sliding")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _service;

        public TodosController(ITodoService service)
        {
            _service = service;
        }

        // Helper method to extract user ID from JWT claims
        private int GetUserId() 
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Retrieves a paged list of todos for the authenticated user.
        /// </summary>
        /// <param name="page">Page number (1-based). Defaults to 1.</param>
        /// <param name="pageSize">Number of items per page. Defaults to 10.</param>
        /// <param name="search">Optional search term to filter by title.</param>
        /// <returns>Returns 200 OK with a list of <see cref="TodoResponseDto"/> items belonging to the authenticated user.</returns>
        /// <response code="200">Successfully retrieved a page of todos.</response>
        /// <response code="401">If the request is not authenticated or the JWT token is invalid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
        [HttpGet]
        [MapToApiVersion(1.0)]
        public async Task<IActionResult> GetTodos(int page = 1, int pageSize = 10, string? search = null)
        {
            int userId = GetUserId();
            
            var todos = await _service.GetAllAsync(page, pageSize, search, userId);

            return Ok(todos);
        }
        [HttpGet("v2")]
        [MapToApiVersion(2.0)]
        public async Task<IActionResult> GetTodosV2(
         int page = 1,
         int pageSize = 10,
         string? search = null,
         bool? isDone = null)
        {
            int userId = GetUserId();

            var todos = await _service.GetAllV2Async(page, pageSize, search, isDone, userId);

            return Ok(todos);
        }
        /// <summary>
        /// Retrieves a single todo by id for the authenticated user.
        /// </summary>
        /// <param name="id">The id of the todo to fetch.</param>
        /// <returns>Returns 200 OK with the requested <see cref="TodoResponseDto"/> if it belongs to the authenticated user.</returns>
        /// <response code="200">Todo item found and returned.</response>
        /// <response code="404">Todo item with the specified id was not found (or does not belong to the authenticated user).</response>
        /// <response code="401">If the request is not authenticated or the JWT token is invalid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodo(int id)
        {
            int userId = GetUserId();

            var todo = await _service.GetByIdAsync(id, userId);
            if(todo == null)
                return NotFound();

            return Ok(todo);
        }

        /// <summary>
        /// Creates a new todo item for the authenticated user.
        /// </summary>
        /// <param name="dto">The create DTO containing the todo data.</param>
        /// <returns>Returns 201 Created with the created <see cref="TodoResponseDto"/>.</returns>
        /// <response code="201">Todo was created successfully.</response>
        /// <response code="400">If the supplied DTO is invalid.</response>
        /// <response code="401">If the request is not authenticated or the JWT token is invalid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
        [HttpPost]
        public async Task<IActionResult> CreateTodo(CreateTodoDto dto)
        {
            int userId = GetUserId();
            var createdTodo = await _service.CreateTodoAsync(dto, userId);

            if (createdTodo == null) return BadRequest();

            return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
        }

        /// <summary>
        /// Updates an existing todo item for the authenticated user.
        /// </summary>
        /// <param name="id">The id of the todo to update.</param>
        /// <param name="dto">The update DTO containing the new values.</param>
        /// <returns>Returns 204 No Content on success.</returns>
        /// <response code="204">Update successful.</response>
        /// <response code="400">If the supplied DTO is invalid.</response>
        /// <response code="404">If no todo with the given id exists (or it does not belong to the authenticated user).</response>
        /// <response code="401">If the request is not authenticated or the JWT token is invalid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, UpdateTodoDto dto)
        {
            int userId = GetUserId();
            var updated = await _service.UpdateTodoAsync(id, dto, userId);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes a todo item by id for the authenticated user.
        /// </summary>
        /// <param name="id">The id of the todo to delete.</param>
        /// <returns>Returns 204 No Content on success.</returns>
        /// <response code="204">Delete successful.</response>
        /// <response code="404">If no todo with the given id exists (or it does not belong to the authenticated user).</response>
        /// <response code="401">If the request is not authenticated or the JWT token is invalid.</response>
        /// <response code="429">When rate limit is exceeded.</response>
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