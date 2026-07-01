using SkyRoutePlatform.API.Features.Bookings.Contracts;
using SkyRoutePlatform.API.Features.Bookings.Services;
using SkyRoutePlatform.API.Shared.Responses;

namespace SkyRoutePlatform.API.Features.Bookings.Endpoints;

/// <summary>
/// Endpoint for retrieving booking details by reference code.
/// </summary>
public sealed class GetBookingEndpoint
{
    /// <summary>
    /// Handles the get booking request.
    /// </summary>
    /// <param name="bookingReference">The booking reference code</param>
    /// <param name="bookingService">The booking service</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Booking details or error response (via ApiResponse)</returns>
    public static async Task<IResult> HandleAsync(string bookingReference, IBookingService bookingService, CancellationToken cancellationToken)
    {
        // Call service and match response to IResult
        ApiResponse<GetBookingResponse> response = await bookingService.GetBookingByReferenceAsync(
            bookingReference,
            cancellationToken
        );

        return response.Match(
            success => Results.Ok(response),
            error => response.ToErrorResult()
        );
    }
}

/// <summary>
/// Extension methods for registering the get booking endpoint.
/// </summary>
public static class GetBookingEndpointExtensions
{
    /// <summary>
    /// Maps the get booking endpoint.
    /// </summary>
    public static void MapGetBookingEndpoint(this WebApplication app)
    {
        app.MapGet("/api/bookings/{bookingReference}", GetBookingEndpoint.HandleAsync)
            .WithName("GetBooking")
            .WithDescription("Retrieve booking details by reference code.");
    }
}
