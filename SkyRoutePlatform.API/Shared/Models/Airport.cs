namespace SkyRoutePlatform.API.Shared.Models;

/// <summary>
/// Represents an airport with location and country information.
/// </summary>
/// <param name="Code">IATA airport code (e.g., JFK, LHR)</param>
/// <param name="City">City name where the airport is located</param>
/// <param name="CountryCode">ISO 3166-1 alpha-2 country code</param>
/// <param name="CountryName">Full country name</param>
public sealed record Airport(string Code, string City, string CountryCode, string CountryName);
