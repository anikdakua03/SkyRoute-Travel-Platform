using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Flights.Providers;

/// <summary>
/// Abstraction for flight providers that can supply flight offers.
/// </summary>
public interface IFlightProvider
{
    /// <summary>
    /// Gets the provider type this implementation represents.
    /// </summary>
    ProviderType ProviderType { get; }

    /// <summary>
    /// Searches for available flights matching the given criteria.
    /// </summary>
    /// <param name="originCode">IATA airport code for origin</param>
    /// <param name="destinationCode">IATA airport code for destination</param>
    /// <param name="departureDate">Date of departure in YYYY-MM-DD format</param>
    /// <param name="passengers">Number of passengers</param>
    /// <param name="cabinClass">Cabin class to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available flight offers from this provider</returns>
    Task<IEnumerable<ProviderFlightOffer>> SearchFlightsAsync(string originCode, string destinationCode, string departureDate, int passengers, CabinClass cabinClass, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific flight offer by its ID.
    /// </summary>
    /// <param name="flightId">The flight ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The flight offer if found; otherwise null</returns>
    Task<ProviderFlightOffer?> GetFlightByIdAsync(string flightId, CancellationToken cancellationToken = default);
}
