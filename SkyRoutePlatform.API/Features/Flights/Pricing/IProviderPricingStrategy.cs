namespace SkyRoutePlatform.API.Features.Flights.Pricing;

/// <summary>
/// Abstraction for provider-specific pricing strategies.
/// </summary>
public interface IProviderPricingStrategy
{
    /// <summary>
    /// Calculates the final per-passenger price for a given base fare.
    /// </summary>
    /// <param name="baseFare">The base fare to apply pricing rules to</param>
    /// <returns>The calculated per-passenger price after applying provider-specific rules</returns>
    decimal CalculatePerPassengerPrice(decimal baseFare);

    /// <summary>
    /// Calculates the total price for all passengers given the per-passenger price.
    /// </summary>
    /// <param name="perPassengerPrice">The price per passenger</param>
    /// <param name="passengers">Total number of passengers</param>
    /// <returns>The total price for all passengers</returns>
    decimal CalculateTotalPrice(decimal perPassengerPrice, int passengers);
}
