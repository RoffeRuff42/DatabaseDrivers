using TodoApi.DTOs;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        Task<List<TodoResponseDto>> GetAllAsync(int page, int pageSize, string? search);
        Task<TodoResponseDto?> GetByIdAsync(int id);
        Task<TodoResponseDto> CreateTodoAsync(CreateTodoDto createTodoDto);
        Task<bool> DeleteTodoAsync(int id);
        Task<bool> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto);
    }
}
