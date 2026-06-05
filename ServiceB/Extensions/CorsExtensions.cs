namespace UserApi.Extensions
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
                        policy.WithOrigins("https://grahnnen.github.io")
                              .AllowAnyMethod()   
                              .AllowAnyHeader();

                    });

                // Policy for development
                options.AddPolicy("DevelopmentPolicy", policy =>
                {
                    policy.WithOrigins(
                           "http://localhost:5500",
                           "http://127.0.0.1:5500")
                          .AllowAnyMethod()   
                          .AllowAnyHeader();

                });
            });
            return services;
        }
    }
}