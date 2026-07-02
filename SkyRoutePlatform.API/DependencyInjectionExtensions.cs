using System.Diagnostics;
using System.Text.Json.Serialization;
using SkyRoutePlatform.API.Features.Bookings.Repositories;
using SkyRoutePlatform.API.Features.Bookings.Services;
using SkyRoutePlatform.API.Features.Flights.Providers;
using SkyRoutePlatform.API.Features.Flights.Services;
using SkyRoutePlatform.API.Middlewares;
using SkyRoutePlatform.API.Shared.Validation;

namespace SkyRoutePlatform.API;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The updated service collection</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register flight providers with data sources
        string globalAirDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "providers", "global-air-flights.json");
        string budgetWingsDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "providers", "budget-wings-flights.json");

        services.AddSingleton<IFlightProvider>(sp => new GlobalAirProviderMock(new JsonFileProviderDataSource(globalAirDataPath)));

        services.AddSingleton<IFlightProvider>(sp => new BudgetWingsProviderMock(new JsonFileProviderDataSource(budgetWingsDataPath)));

        // Register validation services
        services.AddScoped<IDocumentValidator, DocumentValidator>();

        // Register flight services
        services.AddScoped<IFlightSearchService, FlightSearchService>();

        // Register booking services
        services.AddScoped<IBookingRepository, InMemoryBookingRepository>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }

    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The updated service collection</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // Register exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails(config =>
        {
            config.CustomizeProblemDetails = context =>
            {
                string requestId = context.HttpContext.TraceIdentifier;
                string traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                string instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Instance = instance;
                context.ProblemDetails.Extensions["traceId"] = traceId;
                context.ProblemDetails.Extensions["requestId"] = requestId;
            };
        });

        return services;
    }

    /// <summary>
    /// Adds CORS policies to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add CORS policies to</param>
    /// <returns>The updated service collection</returns>
    public static IServiceCollection AddCORSPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Configures the application to use the defined CORS policies.
    /// </summary>
    /// <param name="app">The application builder to configure CORS for</param>
    /// <returns>The updated application builder</returns>
    public static IApplicationBuilder UseCORSPolicies(this IApplicationBuilder app)
    {
        app.UseCors("AllowFrontend");

        return app;
    }
}
