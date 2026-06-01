using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs;

public sealed class AiDescriptionRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Title { get; init; }
}