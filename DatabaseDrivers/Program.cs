
using Scalar.AspNetCore;
using TodoApi.Services;
namespace DatabaseDrivers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddHttpClient<ITodoService, TodoService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7194");
                client.Timeout = TimeSpan.FromMinutes(2);
            })
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(3);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);

                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(4);

                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(5);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            });

            //change to right url and client name after decision
            builder.Services.AddHttpClient("ExternalApi", client =>
            {
                client.BaseAddress = new Uri("https://external-service.com");
                client.Timeout = TimeSpan.FromMinutes(2);
            })
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(3);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);

                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(4);

                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(5);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
