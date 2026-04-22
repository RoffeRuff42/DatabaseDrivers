using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Scalar.AspNetCore;
using TodoApi.Clients;
using TodoApi.Data;
using TodoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DatabaseDrivers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // JWT Authentication Configuration
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            // Using SQLite as the database provider
            builder.Services.AddDbContext<TodoDbContext>(options =>
                options.UseSqlite("Data Source=todo_app.db"));

            // JWT Authentication and Authorization
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
                    };
                });

            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddScoped<ITodoService, TodoService>(); // Changed from AddSingleton to AddScoped for better handling of DbContext
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
                var baseUrl = builder.Configuration["ExternalApiConfig:BaseUrl"] ?? throw new InvalidOperationException("External API base URL is not configured.");
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
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
                db.Database.EnsureCreated();
            }
            app.UseHttpsRedirection();
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
