using TodoApi.DTOs;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        Task<List<TodoResponseDto>> GetAllAsync(int page, int pageSize, string? search, string ticketId);
        Task<TodoResponseDto?> GetByIdAsync(int id, string ticketId);
        Task<TodoResponseDto?> CreateTodoAsync(CreateTodoDto createTodoDto, string ticketId);
        Task<bool> DeleteTodoAsync(int id, string ticketId);
        Task<bool> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto, string ticketId);
    }
}
