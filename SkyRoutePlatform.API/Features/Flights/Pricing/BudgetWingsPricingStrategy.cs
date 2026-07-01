namespace SkyRoutePlatform.API.Features.Flights.Pricing;

/// <summary>
/// Pricing strategy for BudgetWings.
/// Rule: final per-passenger fare = base fare - 10% discount, with minimum fare floor of 29.99.
/// Discount applied only to base fare, and result cannot be below minimum.
/// </summary>
public sealed class BudgetWingsPricingStrategy : IProviderPricingStrategy
{
    private const decimal DiscountPercentage = 0.10m;
    private const decimal MinimumFareFloor = 29.99m;

    /// <summary>
    /// Calculates the per-passenger price by applying a 10% discount with a minimum floor.
    /// </summary>
    /// <param name="baseFare">The base fare</param>
    /// <returns>Base fare minus 10% discount, but not below 29.99, rounded to 2 decimals</returns>
    public decimal CalculatePerPassengerPrice(decimal baseFare)
    {
        decimal discount = baseFare * DiscountPercentage;
        decimal discountedPrice = baseFare - discount;
        decimal finalPrice = Math.Max(discountedPrice, MinimumFareFloor);

        return Math.Round(finalPrice, 2);
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
