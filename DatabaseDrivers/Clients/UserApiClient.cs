using System.Security.Cryptography.X509Certificates;

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

        //public async Task<UserLoginDto> UserLoginAsync(string username, string password)
        //{
        //    try
        //    {
        //        var loginData = new { Username = username, Password = password };
        //        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginData);
        //        response.EnsureSuccessStatusCode();
        //        return await response.Content.ReadFromJsonAsync<UserLoginDto>();
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        // Log the error for debugging purposes
        //        _logger.LogError(ex, $"[UserApiClient Error]: Failed to login user {username}");
        //        throw;
        //    }
        //}
    }
}
