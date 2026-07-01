using SkyRoutePlatform.API.Features.Flights.Contracts;
using SkyRoutePlatform.API.Features.Flights.Services;
using SkyRoutePlatform.API.Shared.Responses;

namespace SkyRoutePlatform.API.Features.Flights.Endpoints;

/// <summary>
/// Endpoint for searching flights across all providers.
/// </summary>
public sealed class SearchFlightsEndpoint
{
    /// <summary>
    /// Handles the search flights request.
    /// </summary>
    /// <param name="request">The search request</param>
    /// <param name="searchService">The flight search service</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with pricing or error response (via ApiResponse)</returns>
    public static async Task<IResult> HandleAsync(SearchFlightsRequest request, IFlightSearchService searchService, CancellationToken cancellationToken)
    {
        // Call service and match response to IResult
        ApiResponse<SearchFlightsResponse> response = await searchService.SearchFlightsAsync(
            request.OriginCode,
            request.DestinationCode,
            request.DepartureDate,
            request.Passengers,
            request.CabinClass,
            cancellationToken
        );

        return response.Match(
            success => Results.Ok(response),
            error => response.ToErrorResult()
        );
    }
}

/// <summary>
/// Extension methods for registering the search flights endpoint.
/// </summary>
public static class SearchFlightsEndpointExtensions
{
    /// <summary>
    /// Maps the search flights endpoint.
    /// </summary>
    public static void MapSearchFlightsEndpoint(this WebApplication app)
    {
        app.MapPost("/api/flights/search", SearchFlightsEndpoint.HandleAsync)
            .WithName("SearchFlights")
            .WithDescription("Search for flights across all providers with normalized pricing.");
    }
}
