using SkyRoutePlatform.API.Features.Flights.Contracts;
using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Flights.Mappings;

/// <summary>
/// Extension methods for converting provider flight offers to API response DTOs.
/// </summary>
public static class FlightDetailDtoExtensions
{
    /// <summary>
    /// Converts a FlightDetail to a response DTO for API consumers.
    /// </summary>
    /// <param name="flight">The flight detail</param>
    /// <returns>Response DTO ready for API serialization</returns>
    public static FlightResultDto ToFlightResultDto(this FlightDetail flight)
    {
        return new FlightResultDto(
            FlightId: flight.FlightId,
            Provider: flight.Provider.ToString(),
            FlightNumber: flight.FlightNumber,
            OriginCode: flight.OriginCode,
            DestinationCode: flight.DestinationCode,
            DepartureTimeUtc: flight.DepartureTimeUtc,
            ArrivalTimeUtc: flight.ArrivalTimeUtc,
            DurationMinutes: flight.DurationMinutes,
            CabinClass: flight.CabinClass.ToString(),
            PerPassengerPrice: flight.PerPassengerPrice,
            TotalPrice: flight.TotalPrice
        );
    }
}
