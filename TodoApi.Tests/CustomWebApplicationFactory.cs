using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TodoApi.Clients;
using TodoApi.Data;
using TodoApi.Models;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserApiClient>();
            services.RemoveAll<IExternalApiClient>();

            services.AddScoped<IUserApiClient, FakeUserApiClient>();
            services.AddScoped<IExternalApiClient, FakeExternalApiClient>();
        });
    }
}