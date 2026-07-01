using SkyRoutePlatform.API.Features.Flights.Contracts;
using SkyRoutePlatform.API.Shared.Responses;

namespace SkyRoutePlatform.API.Features.Flights.Services;

/// <summary>
/// Interface for flight search operations across multiple providers.
/// Services return ApiResponse<T> with both success and error cases.
/// </summary>
public interface IFlightSearchService
{
    /// <summary>
    /// Searches for flights with all validation and pricing applied.
    /// Returns wrapped in ApiResponse with success or error details.
    /// </summary>
    /// <param name="originCode">Three-letter airport code for origin</param>
    /// <param name="destinationCode">Three-letter airport code for destination</param>
    /// <param name="departureDate">Departure date in YYYY-MM-DD format</param>
    /// <param name="passengers">Number of passengers (1-9)</param>
    /// <param name="cabinClass">Cabin class as string (Economy, Business, FirstClass)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ApiResponse with SearchFlightsResponse on success or error details on failure</returns>
    Task<ApiResponse<SearchFlightsResponse>> SearchFlightsAsync(string originCode, string destinationCode, string departureDate, int passengers, string cabinClass, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific flight by ID with pricing applied.
    /// Returns wrapped in ApiResponse with success or error details.
    /// </summary>
    /// <param name="flightId">The flight ID</param>
    /// <param name="passengers">Number of passengers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ApiResponse with FlightResultDto on success or error details on failure</returns>
    Task<ApiResponse<FlightResultDto>> GetFlightByIdAsync(string flightId, int passengers, CancellationToken cancellationToken = default);
}
