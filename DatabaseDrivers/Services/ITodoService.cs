using TodoApi.DTOs;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        // Notice: All ticketId parameters are removed
        Task<List<TodoResponseDto>> GetAllAsync(int page, int pageSize, string? search, int userId);
        Task<TodoResponseDto?> GetByIdAsync(int id, int userId);
        Task<TodoResponseDto?> CreateTodoAsync(CreateTodoDto createTodoDto, int userId);
        Task<bool> DeleteTodoAsync(int id, int userId);
        Task<bool> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto, int userId);

        Task<List<TodoResponseV2Dto>> GetAllV2Async(int page, int pageSize, string? search, bool? isDone, int userId);
    }
}
