using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Scalar.AspNetCore;
using TodoApi.Clients;
using TodoApi.Data;
using TodoApi.Filters;
using TodoApi.Extensions;
using TodoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Using SQLite as the database provider
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todo_app.db"));

// JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

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
builder.Services.AddScoped<ExecutionTimeFilter>();
builder.Services.AddScoped<ITodoService, TodoService>(); // Changed from AddSingleton to AddScoped for better handling of DbContext
//Cache
builder.Services.AddMemoryCache();


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExecutionTimeFilter>(); // Add the execution time filter globally to all controllers
});

builder.Services.AddOpenApi();
builder.Services.AddCustomCors(builder.Configuration);

// Adds standardized error responses (ProblemDetails)
builder.Services.AddProblemDetails(options => {
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path; // Include the request path in the error response
    };
});


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
//change to right url and client name in appsettings after decision
builder.Services.AddHttpClient<IQuoteService, QuoteService>(client =>
{
    client.BaseAddress = new Uri("https://zenquotes.io/");
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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();
  app.UseCors("DevelopmentPolicy");
 }
 else
 {
   app.UseCors("ProductionPolicy");
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

