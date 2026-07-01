using SkyRoutePlatform.API.Features.Bookings.Contracts;
using SkyRoutePlatform.API.Features.Bookings.Services;
using SkyRoutePlatform.API.Shared.Responses;

namespace SkyRoutePlatform.API.Features.Bookings.Endpoints;

/// <summary>
/// Endpoint for creating flight bookings.
/// </summary>
public sealed class CreateBookingEndpoint
{
    /// <summary>
    /// Handles the create booking request.
    /// </summary>
    /// <param name="request">The booking request</param>
    /// <param name="bookingService">The booking service</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Booking confirmation or error response (via ApiResponse)</returns>
    public static async Task<IResult> HandleAsync(CreateBookingRequest request, IBookingService bookingService, CancellationToken cancellationToken)
    {
        // Call service and match response to IResult
        ApiResponse<CreateBookingResponse> response = await bookingService.CreateBookingAsync(
            request.FlightId,
            request.CabinClass,
            request.Passengers,
            request.FullName,
            request.Email,
            request.DocumentType,
            request.DocumentNumber,
            cancellationToken
        );

        return response.Match(
            success => Results.Created($"/bookings/{response.Data?.BookingReference}", response),
            error => response.ToErrorResult()
        );
    }
}

/// <summary>
/// Extension methods for registering the create booking endpoint.
/// </summary>
public static class CreateBookingEndpointExtensions
{
    /// <summary>
    /// Maps the create booking endpoint.
    /// </summary>
    public static void MapCreateBookingEndpoint(this WebApplication app)
    {
        app.MapPost("/api/bookings", CreateBookingEndpoint.HandleAsync)
            .WithName("CreateBooking")
            .WithDescription("Create a new flight booking with passenger details.");
    }
}
