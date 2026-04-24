using TodoApi.Clients;

public class FakeExternalApiClient : IExternalApiClient
{
    public Task<object?> GetTestDataAsync(string endpoint)
    {
        // Return fake data instead of calling real external API
        return Task.FromResult<object?>(new { message = "fake data" });
    }
}