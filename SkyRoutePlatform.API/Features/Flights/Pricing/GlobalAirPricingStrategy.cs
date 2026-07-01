namespace SkyRoutePlatform.API.Features.Flights.Pricing;

/// <summary>
/// Pricing strategy for GlobalAir.
/// Rule: final per-passenger fare = base fare + 15% fuel surcharge, rounded to 2 decimals.
/// </summary>
public sealed class GlobalAirPricingStrategy : IProviderPricingStrategy
{
    private const decimal FuelSurchargePercentage = 0.15m;

    /// <summary>
    /// Calculates the per-passenger price by applying a 15% fuel surcharge.
    /// </summary>
    /// <param name="baseFare">The base fare</param>
    /// <returns>Base fare with 15% surcharge, rounded to 2 decimals</returns>
    public decimal CalculatePerPassengerPrice(decimal baseFare)
    {
        decimal surcharge = baseFare * FuelSurchargePercentage;
        decimal totalPrice = baseFare + surcharge;

        return Math.Round(totalPrice, 2);
    }

    /// <summary>
    /// Calculates the total price for all passengers.
    /// </summary>
    /// <param name="perPassengerPrice">The per-passenger price</param>
    /// <param name="passengers">Number of passengers</param>
    /// <returns>Total price rounded to 2 decimals</returns>
    public decimal CalculateTotalPrice(decimal perPassengerPrice, int passengers)
    {
        decimal totalPrice = perPassengerPrice * passengers;

        return Math.Round(totalPrice, 2);
    }
}
