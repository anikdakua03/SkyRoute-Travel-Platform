using SkyRoutePlatform.API.Features.Flights.Providers.Dtos;
using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Flights.Mappings;

/// <summary>
/// Extension methods for BudgetWings flight DTO normalization to unified model.
/// </summary>
public static class BudgetWingsFlightDtoExtensions
{
    /// <summary>
    /// Converts a BudgetWings DTO to a provider flight offer.
    /// </summary>
    /// <param name="dto">The BudgetWings flight DTO</param>
    /// <returns>Normalized provider flight offer</returns>
    public static ProviderFlightOffer ToProviderFlightOffer(this BudgetWingsFlightDto dto)
    {
        if (!Enum.TryParse<CabinClass>(dto.CabinClass, ignoreCase: true, out CabinClass cabinClass))
        {
            throw new InvalidOperationException($"Unsupported cabin class: {dto.CabinClass}");
        }

        int durationMinutes = (int)(dto.ArrivalTimeUtc - dto.DepartureTimeUtc).TotalMinutes;

        return new ProviderFlightOffer(
            FlightId: dto.FlightId,
            Provider: ProviderType.BudgetWings,
            FlightNumber: dto.FlightNumber,
            OriginCode: dto.OriginCode,
            DestinationCode: dto.DestinationCode,
            DepartureTimeUtc: dto.DepartureTimeUtc,
            ArrivalTimeUtc: dto.ArrivalTimeUtc,
            DurationMinutes: durationMinutes,
            CabinClass: cabinClass,
            BaseFare: dto.BaseFare,
            Currency: dto.Currency
        );
    }
}
