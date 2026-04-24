using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;
using TodoApi.Clients;
using TodoApi.Data;
using TodoApi.Extensions;
using TodoApi.Filters;
using TodoApi.Services;


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
// Adds standardized error responses (ProblemDetails)
builder.Services.AddProblemDetails(options => {
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path; // Include the request path in the error response
    };
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExecutionTimeFilter>(); // Add the execution time filter globally to all controllers
});

//change to right url and client name in appsettings after decision
builder.Services.AddHttpClient<IQuoteService, QuoteService>(client =>
{
    client.BaseAddress = new Uri("https://zenquotes.io/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromSeconds(2);
    options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
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


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
}); 
builder.Services.AddCustomCors(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
        
app.Run();   

public partial class Program { }

internal sealed class BearerSecuritySchemeTransformer(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
            }
        }
    }
}