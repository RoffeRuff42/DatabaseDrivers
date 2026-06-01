using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TodoApi.DTOs;
using TodoApi.Options;

namespace TodoApi.Clients;

public sealed class OpenAiClient : IAiClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiClient> _logger;

    public OpenAiClient(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "Missing OpenAi:ApiKey. Store it with user-secrets or environment variables.");
        }

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<AiDescriptionResponseDto> GenerateDescriptionAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _options.Model,
            instructions = """
            You generate short todo descriptions.
            Return only the description.
            Do not use markdown.
            Maximum 2 short sentences.
            """,
            input = $"Create a helpful description for this todo title: \"{title}\"",
            max_output_tokens = _options.MaxOutputTokens
        };

        using var response = await _httpClient.PostAsJsonAsync(
            _options.Endpoint,
            requestBody,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogWarning("OpenAI request failed. Status: {StatusCode}. Body: {Body}", (int)response.StatusCode, errorBody);

            throw new HttpRequestException(
                $"OpenAI request failed with status {(int)response.StatusCode}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var description = ExtractText(document.RootElement);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOperationException("OpenAI returned no description text.");
        }

        var model = document.RootElement.TryGetProperty("model", out var modelProperty)
            ? modelProperty.GetString() ?? _options.Model
            : _options.Model;

        return new AiDescriptionResponseDto
        {
            Description = description.Trim(),
            Model = model,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private static string ExtractText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputText)
            && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? string.Empty;
        }

        var textParts = new List<string>();

        if (!root.TryGetProperty("output", out var outputArray)
            || outputArray.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var outputItem in outputArray.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var contentArray)
                || contentArray.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in contentArray.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var textProperty)
                    && textProperty.ValueKind == JsonValueKind.String)
                {
                    var value = textProperty.GetString();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        textParts.Add(value);
                    }
                }
            }
        }

        return string.Join(Environment.NewLine, textParts);
    }
}