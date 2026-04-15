using TodoApi.DTOs;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        List<TodoResponseDto> GetAll();
        TodoResponseDto? GetById(int id);
        TodoResponseDto Create(CreateTodoDto createTodoDto);
        bool Update(int id, UpdateTodoDto updateTodoDto);
        bool Delete(int id);
    }
}
