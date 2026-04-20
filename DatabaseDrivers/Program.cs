using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using TodoApi.Clients;
using TodoApi.Services;
using Microsoft.Extensions.Http.Resilience;

namespace DatabaseDrivers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<ITodoService, TodoService>();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddHttpClient<IQuoteService, QuoteService>(client =>
            {
                client.BaseAddress = new Uri("https://zenquotes.io/");
            });

            // Adds standardized error responses (ProblemDetails)
            builder.Services.AddProblemDetails(options => {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = context.HttpContext.Request.Path; // Include the request path in the error response
                };
            });

            builder.Services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
            {
                var baseUrl = builder.Configuration["Services:UserApi"] ?? throw new InvalidOperationException("User API base URL is not configured.");
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            });

            //change to right url and client name in appsettings after decision
            builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>(client =>
            {
                var baseUrl = builder.Configuration["Services:ExternalApi"] ?? throw new InvalidOperationException("External API base URL is not configured.");
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            });
            //Cache
            builder.Services.AddMemoryCache();
            //Ratelimiting
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddSlidingWindowLimiter("sliding", config =>
                {
                    config.Window = TimeSpan.FromMinutes(1);
                    config.SegmentsPerWindow = 6;
                    config.PermitLimit = 100;
                    config.QueueLimit = 2;
                });
                //ADD THIS TO CONTROLLER
                //[EnableRateLimiting("sliding")]
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();
            app.UseRateLimiter();

            app.MapControllers();

            app.Run();
        }
    }
}
