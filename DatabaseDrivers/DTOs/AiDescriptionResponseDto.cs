namespace TodoApi.DTOs;

public sealed class AiDescriptionResponseDto
{
    public required string Description { get; init; }
    public required string Model { get; init; }
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
}