using System.Net.Http.Json;

namespace TodoApi.Clients;

public class ExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public ExternalApiClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;

        // Fetch values from User Secrets or Environment Variables
        var baseUrl = _config["ExternalApiConfig:BaseUrl"];
        var apiKey = _config["ExternalApiConfig:ApiKey"];

        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        // We add the key to the headers so it's sent with every request
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }
    }



    // Fetches data from the external API. 
    // Handles network errors and ensures a successful status code.
    public async Task<object?> GetTestDataAsync(string endpoint) // Change when we are sure of an api
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<object>();
        }
        catch (HttpRequestException ex)
        {
            // Log the error for debugging purposes
            Console.WriteLine($"[ExternalApiClient Error]: {ex.Message}");
            throw;
        }
    }
}
