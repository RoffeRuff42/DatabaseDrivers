using System.Net.Http.Json;

namespace TodoApi.Clients;

//public class ExternalApiClient : IExternalApiClient
//{
//    private readonly HttpClient _httpClient;
//    private readonly IConfiguration _config;
//    private readonly ILogger<ExternalApiClient> _logger;

//    public ExternalApiClient(HttpClient httpClient, IConfiguration config, ILogger<ExternalApiClient> logger)
//    {
//        _httpClient = httpClient;
//        _config = config;
//        _logger = logger;

//        // Fetch values from User Secrets or Environment Variables
//        var headerName = _config["ExternalApiConfig:HeaderName"] ?? throw new ArgumentNullException("External API header name is missing"); 
//        var apiKey = _config["ExternalApiConfig:ApiKey"] ?? throw new ArgumentNullException("External API Key is missing");

//        // We add the key to the headers so it's sent with every request
//        if (!string.IsNullOrEmpty(headerName) && !string.IsNullOrEmpty(apiKey))
//        {
//            _httpClient.DefaultRequestHeaders.Add(headerName, apiKey);
//        }
//    }

//    // Fetches data from the external API. 
//    // Handles network errors and ensures a successful status code.
//    public async Task<object?> GetTestDataAsync(string endpoint) // Change when we are sure of an api
//    {
//        try
//        {
//            var response = await _httpClient.GetAsync(endpoint);

//            response.EnsureSuccessStatusCode();

//            return await response.Content.ReadFromJsonAsync<object>();
//        }
//        catch (HttpRequestException ex)
//        {
//            // Log the error for debugging purposes
//            _logger.LogError(ex, $"[ExternalApiClient Error]: Failed to fetch data from endpoint {endpoint}");
//            throw;
//        }
//    }
//}
