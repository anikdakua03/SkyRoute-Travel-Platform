namespace SkyRoutePlatform.API.Features.Flights.Providers.Dtos;

/// <summary>
/// Data transfer object for GlobalAir flight records from their JSON data source.
/// </summary>
public sealed record GlobalAirFlightDto(string FlightId, string FlightNumber, string Origin, string Destination, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, string Cabin, decimal BaseFare, string Currency);
