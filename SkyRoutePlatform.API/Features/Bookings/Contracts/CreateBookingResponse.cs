namespace SkyRoutePlatform.API.Features.Bookings.Contracts;

/// <summary>
/// Response contract for booking confirmation.
/// </summary>
public sealed record CreateBookingResponse(string BookingReference, string Status);
