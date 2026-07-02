using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Bookings.Repositories;

/// <summary>
/// Repository abstraction for booking persistence operations.
/// Allows implementation swapping between in-memory, SQL, NoSQL, etc.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Creates a new booking in the repository.
    /// </summary>
    /// <param name="booking">The booking to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created booking</returns>
    Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a booking by its reference code.
    /// </summary>
    /// <param name="bookingReference">The booking reference code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The booking if found; otherwise null</returns>
    Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);
}
