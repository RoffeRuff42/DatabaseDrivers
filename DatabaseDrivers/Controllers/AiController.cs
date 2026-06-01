using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Clients;
using TodoApi.DTOs;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/v1/ai")]
[Authorize]
public sealed class AiController : ControllerBase
{
    private readonly IAiClient _aiClient;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiClient aiClient, ILogger<AiController> logger)
    {
        _aiClient = aiClient;
        _logger = logger;
    }

    [HttpPost("description")]
    [ProducesResponseType(typeof(AiDescriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<AiDescriptionResponseDto>> GenerateDescription(
        [FromBody] AiDescriptionRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiClient.GenerateDescriptionAsync(
                request.Title,
                cancellationToken);

            return Ok(result);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "AI request timed out.");
            return StatusCode(StatusCodes.Status504GatewayTimeout, "AI request timed out.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "AI provider unavailable.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "AI provider unavailable.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI integration failed.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "AI integration failed.");
        }
    }
}