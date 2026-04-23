namespace TodoApi.Clients
{
    public interface IExternalApiClient
    {
        Task<object?> GetTestDataAsync(string endpoint); // Change when we are sure of an api
    }
}
