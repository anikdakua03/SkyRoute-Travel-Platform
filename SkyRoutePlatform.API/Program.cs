using Scalar.AspNetCore;
using SkyRoutePlatform.API;
using SkyRoutePlatform.API.Features.Bookings.Endpoints;
using SkyRoutePlatform.API.Features.Flights.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

// Add logging
builder.Services.AddLogging();

// Add CORS 
builder.Services.AddCORSPolicies();

WebApplication app = builder.Build();

// Use the exception handler
app.UseExceptionHandler();

app.UseCORSPolicies();

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