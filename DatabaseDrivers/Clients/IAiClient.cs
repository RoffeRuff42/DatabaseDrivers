using TodoApi.DTOs;

namespace TodoApi.Clients;

public interface IAiClient
{
    Task<AiDescriptionResponseDto> GenerateDescriptionAsync(
        string title,
        CancellationToken cancellationToken = default);
}