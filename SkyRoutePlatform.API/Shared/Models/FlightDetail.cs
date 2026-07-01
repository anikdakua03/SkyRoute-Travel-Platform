namespace SkyRoutePlatform.API.Shared.Models;

/// <summary>
/// Represents a normalized flight detail with pricing information.
/// This is the unified model returned to API consumers after provider-specific normalization.
/// </summary>
public sealed record FlightDetail(string FlightId, ProviderType Provider, string FlightNumber, string OriginCode, string DestinationCode, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, int DurationMinutes, CabinClass CabinClass, decimal PerPassengerPrice, decimal TotalPrice, string Currency);