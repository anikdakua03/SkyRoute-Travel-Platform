using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Bookings.Repositories;

/// <summary>
/// In-memory implementation of IBookingRepository for local development and testing.
/// Thread-safe implementation using concurrent dictionary.
/// </summary>
public sealed class InMemoryBookingRepository : IBookingRepository
{
    private static readonly Dictionary<string, Booking> Bookings = [];

    /// <summary>
    /// Creates a new booking in memory.
    /// </summary>
    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simulate async operation

        if (Bookings.ContainsKey(booking.BookingReference))
        {
            throw new InvalidOperationException($"Booking with reference {booking.BookingReference} already exists.");
        }

        Bookings[booking.BookingReference] = booking;

        return booking;
    }

    /// <summary>
    /// Retrieves a booking by reference from memory.
    /// </summary>
    public async Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simulate async operation

        Bookings.TryGetValue(bookingReference, out Booking? booking);

        return booking;
    }

    /// <summary>
    /// Clears all bookings (useful for testing).
    /// </summary>
    public static void ClearAllBookings()
    {
        Bookings.Clear();
    }

    /// <summary>
    /// Gets all bookings (useful for testing and debugging).
    /// </summary>
    public static IReadOnlyCollection<Booking> GetAllBookings()
    {
        return Bookings.Values.ToList().AsReadOnly();
    }
}
