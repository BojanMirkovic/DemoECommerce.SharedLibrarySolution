using eCommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace eCommerce.SharedLibrary.DependencyInjection
{// The SharedServiceContainer class is a utility to standardize and simplify the setup of common services and middleware 
 // in a microservice architecture. It ensures all microservices use the same configurations for logging, database, 
 // authentication, and error handling, promoting consistency and reducing code duplication. By centralizing these 
 // shared concerns, it makes the code easier to maintain, reuse, and scale across multiple services.
    public static class SharedServiceContainer
    {

        // Extension method to add shared services to the DI container.
        public static IServiceCollection AddSharedServices<TContext>
            (this IServiceCollection services, IConfiguration config, string fileName) where TContext : DbContext
        {
            //Add Generic Database context
            // Add the specified DbContext to the services with SQL Server as the database provider.
            // The connection string is fetched from the configuration (e.g., appsettings.json).
            services.AddDbContext<TContext>(option => option.UseSqlServer(
                config
                .GetConnectionString("eCommerceConnection"), // Get the connection string named "eCommerceConnection".
                sqlServerOption =>
                sqlServerOption.EnableRetryOnFailure()));  // Enable automatic retries on transient database errors.

            //configure Serilog logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"<{fileName}-.text",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Add JWT authentication to the services.
            // This method configures the app to use JWT tokens for API authentication.
            JWTAuthenticationScheme.AddJwtAuthenicationScheme(services, config);

            return services;  // Return the IServiceCollection for method chaining.
        }

        // Extension method to configure shared middleware for the app.
        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            // Add global exception handling middleware to the request pipeline.
            // This middleware captures and handles all unhandled exceptions globally.
            app.UseMiddleware<GlobalException>();

            // Add custom middleware to restrict access to only API Gateway calls.
            // This middleware blocks unauthorized calls from external sources.

            //app.UseMiddleware<ListenToOnlyApiGateway>();// comment until we build Api Gateway

            return app;  // Return the IApplicationBuilder for method chaining.
        }
    }
}
