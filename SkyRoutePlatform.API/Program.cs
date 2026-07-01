using Scalar.AspNetCore;
using SkyRoutePlatform.API.Features.Bookings.Endpoints;
using SkyRoutePlatform.API.Features.Bookings.Repositories;
using SkyRoutePlatform.API.Features.Bookings.Services;
using SkyRoutePlatform.API.Features.Flights.Endpoints;
using SkyRoutePlatform.API.Features.Flights.Providers;
using SkyRoutePlatform.API.Features.Flights.Services;
using SkyRoutePlatform.API.Middlewares;
using SkyRoutePlatform.API.Shared.Validation;
using System.Diagnostics;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Register exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails(config =>
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

// Register flight providers with data sources
string globalAirDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "providers", "global-air-flights.json");
string budgetWingsDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "providers", "budget-wings-flights.json");

builder.Services.AddSingleton<IFlightProvider>(sp => new GlobalAirProviderMock(new JsonFileProviderDataSource(globalAirDataPath)));

builder.Services.AddSingleton<IFlightProvider>(sp => new BudgetWingsProviderMock(new JsonFileProviderDataSource(budgetWingsDataPath)));

// Register validation services
builder.Services.AddScoped<IDocumentValidator, DocumentValidator>();

// Register flight services
builder.Services.AddScoped<IFlightSearchService, FlightSearchService>();

// Register booking services
builder.Services.AddScoped<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Add logging
builder.Services.AddLogging();

WebApplication app = builder.Build();

// Use the exception handler
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapSearchFlightsEndpoint();
app.MapCreateBookingEndpoint();
app.MapGetBookingEndpoint();

app.Run();