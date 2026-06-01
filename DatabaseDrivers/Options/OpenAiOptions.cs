namespace TodoApi.Options;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAi";

    public string BaseUrl { get; set; } = "https://api.openai.com";
    public string Endpoint { get; set; } = "/v1/responses";
    public string Model { get; set; } = "gpt-5.5";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxOutputTokens { get; set; } = 150;
}