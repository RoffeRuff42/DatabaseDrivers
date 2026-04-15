using TodoApi.DTOs;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        List<TodoResponseDto> GetAll(int page, int pageSize, string? search);
        TodoResponseDto? GetById(int id);
        TodoResponseDto Create(CreateTodoDto createTodoDto);
        bool DeleteTodo(int id);
        bool UpdateTodo(int id, UpdateTodoDto updateTodoDto);
    }
}
