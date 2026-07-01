namespace SkyRoutePlatform.API.Features.Flights.Contracts;

/// <summary>
/// Response DTO for displaying a flight result to API consumers.
/// </summary>
public sealed record FlightResultDto(string FlightId, string Provider, string FlightNumber, string OriginCode, string DestinationCode, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, int DurationMinutes, string CabinClass, decimal PerPassengerPrice, decimal TotalPrice);

/// <summary>
/// Response contract for flight search results.
/// </summary>
public sealed record SearchFlightsResponse(string Currency, List<FlightResultDto> Results);
