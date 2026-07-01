using SkyRoutePlatform.API.Features.Flights.Mappings;
using SkyRoutePlatform.API.Features.Flights.Pricing;
using SkyRoutePlatform.API.Features.Flights.Providers.Dtos;
using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Features.Flights.Providers;

/// <summary>
/// Mock provider implementation for BudgetWings.
/// Loads flight data from JSON file and applies BudgetWings pricing rules.
/// </summary>
public sealed class BudgetWingsProviderMock : IFlightProvider
{
    private readonly IProviderDataSource _dataSource;
    private readonly IProviderPricingStrategy _pricingStrategy;

    private readonly List<BudgetWingsFlightDto> _flightCache;

    /// <summary>
    /// Gets the provider type.
    /// </summary>
    public ProviderType ProviderType => ProviderType.BudgetWings;

    /// <summary>
    /// Creates a new instance of BudgetWingsProviderMock.
    /// </summary>
    /// <param name="dataSource">The data source to load BudgetWings flights from</param>
    public BudgetWingsProviderMock(IProviderDataSource dataSource)
    {
        _dataSource = dataSource;
        _pricingStrategy = new BudgetWingsPricingStrategy();
        _flightCache = [.. _dataSource.Load<BudgetWingsFlightDto>()];
    }


    /// <summary>
    /// Searches for BudgetWings flights matching the given criteria.
    /// </summary>
    public async Task<IEnumerable<ProviderFlightOffer>> SearchFlightsAsync(string originCode, string destinationCode, string departureDate, int passengers, CabinClass cabinClass, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // Simulate async operation

        if (!DateTime.TryParse(departureDate, out var depDate))
        {
            return [];
        }

        var results = _flightCache
            .Where(f =>
                f.OriginCode.Equals(originCode, StringComparison.OrdinalIgnoreCase) &&
                f.DestinationCode.Equals(destinationCode, StringComparison.OrdinalIgnoreCase) &&
                f.DepartureTimeUtc.Date == depDate.Date &&
                f.CabinClass.Equals(cabinClass.ToString(), StringComparison.OrdinalIgnoreCase)
            )
            .Select(f => f.ToProviderFlightOffer())
            .ToList();

        return results;
    }

    /// <summary>
    /// Gets a specific BudgetWings flight by ID.
    /// </summary>
    public async Task<ProviderFlightOffer?> GetFlightByIdAsync(string flightId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simulate async operation

        var flight = _flightCache.FirstOrDefault(f => f.FlightId == flightId);

        return flight?.ToProviderFlightOffer();
    }

    /// <summary>
    /// Gets the pricing strategy for this provider.
    /// </summary>
    public IProviderPricingStrategy GetPricingStrategy() => _pricingStrategy;
}
