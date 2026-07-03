using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Bookings.Contracts;

/// <summary>
/// Request contract for creating a booking.
/// </summary>
public sealed record CreateBookingRequest(string FlightId, string CabinClass, int Passengers, IReadOnlyList<PassengerDto> PassengersDetails)
{
    /// <summary>
    /// Converts the string cabin class to the enum value.
    /// </summary>
    public CabinClass GetCabinClass()
    {
        return Enum.Parse<CabinClass>(CabinClass, ignoreCase: true);
    }
}
