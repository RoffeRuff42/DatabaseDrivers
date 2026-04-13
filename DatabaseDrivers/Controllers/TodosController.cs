using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs;
using TodoApi.Services; 

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/v1/todos")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _service;

        public TodosController(ITodoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Hämtar alla todos
        /// </summary>
        [HttpGet]
        public IActionResult GetTodos()
        {
            var todos = _service.GetAll();
            return Ok(todos); 
        }

        
        [HttpGet("{id}")]
        public IActionResult GetTodo(int id)
        {
            var todo = _service.GetById(id);

            if (todo == null)
                return NotFound(); 

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
            var updated = _service.Update(id, dto);

            if (!updated)
                return NotFound(); 

            return NoContent(); 
        }

       
        [HttpDelete("{id}")]
        public IActionResult DeleteTodo(int id)
        {
            var deleted = _service.Delete(id);

            if (!deleted)
                return NotFound(); 

            return NoContent(); 
        }
    }
}