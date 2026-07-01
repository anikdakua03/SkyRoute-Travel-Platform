using SkyRoutePlatform.API.Features.Flights.Mappings;
using SkyRoutePlatform.API.Features.Flights.Pricing;
using SkyRoutePlatform.API.Features.Flights.Providers.Dtos;
using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Flights.Providers;

/// <summary>
/// Mock provider implementation for GlobalAir.
/// Loads flight data from JSON file and applies GlobalAir pricing rules.
/// </summary>
public sealed class GlobalAirProviderMock : IFlightProvider
{
    private readonly IProviderDataSource _dataSource;
    private readonly IProviderPricingStrategy _pricingStrategy;

    private readonly List<GlobalAirFlightDto> _flightCache;

    /// <summary>
    /// Gets the provider type.
    /// </summary>
    public ProviderType ProviderType => ProviderType.GlobalAir;

    /// <summary>
    /// Creates a new instance of GlobalAirProviderMock.
    /// </summary>
    /// <param name="dataSource">The data source to load GlobalAir flights from</param>
    public GlobalAirProviderMock(IProviderDataSource dataSource)
    {
        _dataSource = dataSource;
        _pricingStrategy = new GlobalAirPricingStrategy();
        _flightCache = [.. _dataSource.Load<GlobalAirFlightDto>()];
    }


    /// <summary>
    /// Searches for GlobalAir flights matching the given criteria.
    /// </summary>
    public async Task<IEnumerable<ProviderFlightOffer>> SearchFlightsAsync(string originCode, string destinationCode, string departureDate, int passengers, CabinClass cabinClass, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // Simulate async operation

        if (!DateTime.TryParse(departureDate, out var depDate))
        {
            return [];
        }

        List<ProviderFlightOffer> results = _flightCache
            .Where(f =>
                f.Origin.Equals(originCode, StringComparison.OrdinalIgnoreCase) &&
                f.Destination.Equals(destinationCode, StringComparison.OrdinalIgnoreCase) &&
                f.DepartureTimeUtc.Date == depDate.Date &&
                f.Cabin.Equals(cabinClass.ToString(), StringComparison.OrdinalIgnoreCase)
            )
            .Select(f => f.ToProviderFlightOffer())
            .ToList();

        return results;
    }

    /// <summary>
    /// Gets a specific GlobalAir flight by ID.
    /// </summary>
    public async Task<ProviderFlightOffer?> GetFlightByIdAsync(string flightId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simulate async operation

        GlobalAirFlightDto? flight = _flightCache.FirstOrDefault(f => f.FlightId == flightId);

        return flight?.ToProviderFlightOffer();
    }

    /// <summary>
    /// Gets the pricing strategy for this provider.
    /// </summary>
    public IProviderPricingStrategy GetPricingStrategy() => _pricingStrategy;
}
