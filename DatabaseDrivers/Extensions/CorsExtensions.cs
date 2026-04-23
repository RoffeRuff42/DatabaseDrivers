namespace TodoApi.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration config)
        {
            services.AddCors(options =>
            {
                // Policy for production
                options.AddPolicy("ProductionPolicy", policy =>
                {
                    policy.WithOrigins("https://WebWizzardTodoApp.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });

                // Policy for development
                options.AddPolicy("DevelopmentPolicy", policy =>
                {
                    policy.WithOrigins(
                        "https://localhost:7194", // UserApi HTTPS
                        "http://localhost:5226",  // UserApi HTTP
                        "https://localhost:7276", // TodoApi HTTPS
                        "http://localhost:5269"   // TodoApi HTTP
                    )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            return services;
        }
    }
}