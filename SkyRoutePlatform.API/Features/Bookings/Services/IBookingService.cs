namespace SkyRoutePlatform.API.Features.Bookings.Services;

using SkyRoutePlatform.API.Features.Bookings.Contracts;
using SkyRoutePlatform.API.Shared.Responses;

/// <summary>
/// Interface for booking operations.
/// Services return ApiResponse<T> with both success and error cases.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Creates a new booking with validation.
    /// All validation is performed within this method and wrapped in ApiResponse.
    /// </summary>
    /// <param name="flightId">The selected flight ID</param>
    /// <param name="cabinClass">The cabin class for the booking</param>
    /// <param name="passengers">Number of passengers</param>
    /// <param name="fullName">Full name of the passenger</param>
    /// <param name="email">Email address</param>
    /// <param name="documentType">Type of identification document</param>
    /// <param name="documentNumber">Document number/value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ApiResponse with CreateBookingResponse on success or error details on failure</returns>
    Task<ApiResponse<CreateBookingResponse>> CreateBookingAsync(string flightId, string cabinClass, int passengers, string fullName, string email, string documentType, string documentNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a booking by its reference code.
    /// </summary>
    /// <param name="bookingReference">The booking reference code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ApiResponse with GetBookingResponse on success or error details on failure</returns>
    Task<ApiResponse<GetBookingResponse>> GetBookingByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);
}
