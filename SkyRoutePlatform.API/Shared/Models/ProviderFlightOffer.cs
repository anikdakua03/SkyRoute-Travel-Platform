namespace SkyRoutePlatform.API.Shared.Models;

/// <summary>
/// Represents a flight offer from a provider before pricing is applied.
/// This is the intermediate model used during provider normalization.
/// </summary>
public sealed record ProviderFlightOffer(string FlightId, ProviderType Provider, string FlightNumber, string OriginCode, string DestinationCode, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, int DurationMinutes, CabinClass CabinClass, decimal BaseFare, string Currency);