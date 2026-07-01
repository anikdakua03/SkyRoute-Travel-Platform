using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Flights.Contracts;

/// <summary>
/// Request contract for searching flights.
/// </summary>
public sealed record SearchFlightsRequest(string OriginCode, string DestinationCode, string DepartureDate, int Passengers, string CabinClass)
{
    /// <summary>
    /// Converts the string cabin class to the enum value.
    /// </summary>
    public CabinClass GetCabinClass()
    {
        return Enum.Parse<CabinClass>(CabinClass, ignoreCase: true);
    }
}
