namespace TodoApi.Services
{
    using System.Net.Http.Json;
    using TodoApi.DTOs;

    public class QuoteService : IQuoteService
    {
        private readonly HttpClient _httpClient;

        public QuoteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<QuoteDto?> GetRandomQuote()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://zenquotes.io/api/random");

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<List<QuoteDto>>();

                return data?.FirstOrDefault();
            }
            catch (HttpRequestException)
            {
               
                return null;
            }
        }
    }
}
