using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Scalar.AspNetCore;
using TodoApi.Clients;
using TodoApi.Data;
using TodoApi.Services;

namespace DatabaseDrivers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Choose database based on environment
            if (builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddDbContext<TodoDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            }
            else
            {
                builder.Services.AddDbContext<TodoDbContext>(options =>
                    options.UseSqlite("Data Source=todo_app.db"));
            }

            // Add services to the container.
            builder.Services.AddScoped<ITodoService, TodoService>(); // Changed from AddSingleton to AddScoped for better handling of DbContext
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddHttpClient<IQuoteService, QuoteService>(client =>
            {
                client.BaseAddress = new Uri("https://zenquotes.io/");
            });

            // Adds standardized error responses (ProblemDetails)
            builder.Services.AddProblemDetails(options =>
            {
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
            // Only run in normal application (not during tests)
            if (!app.Environment.IsEnvironment("Testing"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
                    db.Database.EnsureCreated();
                }
            }
            app.UseHttpsRedirection();
            app.UseRateLimiter();

            app.MapControllers();

            app.Run();
        }
       
    }
}
public partial class Program { }
