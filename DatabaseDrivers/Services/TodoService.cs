using TodoApi.DTOs;

namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private static readonly List<TodoResponseDto> _todos = new(); // In-memory list used to store all todos (acts as a temporary data store instead of a database)
        private static int _nextId = 1;  // Counter used to generate unique IDs for each todo 

        public List<TodoResponseDto> GetAll(int page, int pageSize, string? search)
        {
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

        public TodoResponseDto? GetById(int id)
        {
            return _todos.FirstOrDefault(t => t.Id == id);
        }
        public TodoResponseDto Create(CreateTodoDto dto)
        {
            var todo = new TodoResponseDto
            {
                Id = _nextId++,
                Title = dto.Title,
                IsDone = false
            };

            _todos.Add(todo);
            return todo;
        }
        public bool UpdateTodo(int id, UpdateTodoDto updateTodoDto)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return false;

            todo.Title = updateTodoDto.Title;
            todo.IsDone = updateTodoDto.IsDone;

            return true;
        }
        public bool DeleteTodo(int id)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return false;

            _todos.Remove(todo);
            return true;
        }
    

    }

}
