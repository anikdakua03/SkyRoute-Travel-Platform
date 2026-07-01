using SkyRoutePlatform.API.Features.Flights.Providers.Dtos;
using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Flights.Mappings;

/// <summary>
/// Extension methods for GlobalAir flight DTO normalization to unified model.
/// </summary>
public static class GlobalAirFlightDtoExtensions
{
    /// <summary>
    /// Converts a GlobalAir DTO to a provider flight offer.
    /// </summary>
    /// <param name="dto">The GlobalAir flight DTO</param>
    /// <returns>Normalized provider flight offer</returns>
    public static ProviderFlightOffer ToProviderFlightOffer(this GlobalAirFlightDto dto)
    {
        if (!Enum.TryParse<CabinClass>(dto.Cabin, ignoreCase: true, out CabinClass cabinClass))
        {
            throw new InvalidOperationException($"Unsupported cabin class: {dto.Cabin}");
        }

        int durationMinutes = (int)(dto.ArrivalTimeUtc - dto.DepartureTimeUtc).TotalMinutes;

        return new ProviderFlightOffer(
            FlightId: dto.FlightId,
            Provider: ProviderType.GlobalAir,
            FlightNumber: dto.FlightNumber,
            OriginCode: dto.Origin,
            DestinationCode: dto.Destination,
            DepartureTimeUtc: dto.DepartureTimeUtc,
            ArrivalTimeUtc: dto.ArrivalTimeUtc,
            DurationMinutes: durationMinutes,
            CabinClass: cabinClass,
            BaseFare: dto.BaseFare,
            Currency: dto.Currency
        );
    }
}
