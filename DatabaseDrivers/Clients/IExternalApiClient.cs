namespace TodoApi.Clients
{
    public interface IExternalApiClient
    {
        Task<object?> GetTestDataAsync(string endpoint); 
    }
}
