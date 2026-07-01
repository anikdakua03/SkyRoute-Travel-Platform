using System.Text.Json.Serialization;

namespace SkyRoutePlatform.API.Features.Flights.Providers.Dtos;

/// <summary>
/// Data transfer object for BudgetWings flight records from their JSON data source.
/// Uses snake_case field names to match the provider's data format.
/// </summary>
public sealed record BudgetWingsFlightDto(
    [property: JsonPropertyName("flight_id")] string FlightId,
    [property: JsonPropertyName("flight_number")] string FlightNumber,
    [property: JsonPropertyName("origin_code")] string OriginCode,
    [property: JsonPropertyName("destination_code")] string DestinationCode,
    [property: JsonPropertyName("departure_time_utc")] DateTime DepartureTimeUtc,
    [property: JsonPropertyName("arrival_time_utc")] DateTime ArrivalTimeUtc,
    [property: JsonPropertyName("cabin_class")] string CabinClass,
    [property: JsonPropertyName("base_fare")] decimal BaseFare,
    [property: JsonPropertyName("currency")] string Currency
);
