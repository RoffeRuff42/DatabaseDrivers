using Microsoft.AspNetCore.Http.HttpResults;
using TodoApi.DTOs;

namespace TodoApi.Clients
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<UserApiClient> _logger;

        public UserApiClient(HttpClient httpClient, IConfiguration config, ILogger<UserApiClient> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;   

            // Fetch values from User Secrets or Environment Variables
            var apiKey = _config["UserApiConfig:InternalApiKey"] ?? throw new ArgumentNullException("Internal Api Key is missing");
           
            // We add the key to the headers so it's sent with every request
            if(!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            {
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }
        }

        public async Task<UserLoginDto?> ValidateTicketAsync(string ticketId)
        {
            var response = await _httpClient.GetAsync($"/api/auth/validate/{ticketId}"); // sending the ticketId to the UserApi for validation

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Validation failed for ticket: {ticketId}. Status: {response.StatusCode}");
                return null; // returning null to indicate validation failure, allowing the caller to handle it gracefully
            }
            return await response.Content.ReadFromJsonAsync<UserLoginDto>();
        }
    }
}
