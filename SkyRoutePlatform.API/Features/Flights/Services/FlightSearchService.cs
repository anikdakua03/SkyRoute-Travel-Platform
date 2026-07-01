using SkyRoutePlatform.API.Features.Flights.Contracts;
using SkyRoutePlatform.API.Features.Flights.Mappings;
using SkyRoutePlatform.API.Features.Flights.Pricing;
using SkyRoutePlatform.API.Features.Flights.Providers;
using SkyRoutePlatform.API.Shared.Models;
using SkyRoutePlatform.API.Shared.Responses;

namespace SkyRoutePlatform.API.Features.Flights.Services;

/// <summary>
/// Service for orchestrating flight searches across multiple providers with pricing applied.
/// Handles all validation, date/enum parsing, and response aggregation.
/// Returns all results wrapped in ApiResponse<T> for consistent error handling.
/// </summary>
public sealed class FlightSearchService : IFlightSearchService
{
    private readonly IEnumerable<IFlightProvider> _providers;
    private readonly Dictionary<ProviderType, IProviderPricingStrategy> _pricingStrategies;

    /// <summary>
    /// Creates a new instance of FlightSearchService.
    /// </summary>
    /// <param name="providers">Collection of flight providers to search across</param>
    public FlightSearchService(IEnumerable<IFlightProvider> providers)
    {
        _providers = providers;
        _pricingStrategies = new Dictionary<ProviderType, IProviderPricingStrategy>
        {
            { ProviderType.GlobalAir, new GlobalAirPricingStrategy() },
            { ProviderType.BudgetWings, new BudgetWingsPricingStrategy() }
        };
    }

    /// <summary>
    /// Searches for flights with all validation and pricing applied.
    /// Returns ApiResponse with success or error details.
    /// </summary>
    public async Task<ApiResponse<SearchFlightsResponse>> SearchFlightsAsync(string originCode, string destinationCode, string departureDate, int passengers, string cabinClass, CancellationToken cancellationToken = default)
    {
        // ===== FIELD VALIDATION =====
        string? fieldValidationError = ValidateSearchRequestFields(originCode, destinationCode, departureDate, passengers, cabinClass);

        if (fieldValidationError is not null)
        {
            return ApiResponse<SearchFlightsResponse>.Failure(StatusCodes.Status400BadRequest, fieldValidationError);
        }

        // ===== DATE AND ENUM PARSING =====
        if (!DateTime.TryParse(departureDate, out DateTime parsedDepartureDate))
        {
            return ApiResponse<SearchFlightsResponse>.Failure(
                StatusCodes.Status400BadRequest,
                $"Invalid departure date format '{departureDate}'. Date must be in YYYY-MM-DD format."
            );
        }

        if (!Enum.TryParse<CabinClass>(cabinClass, ignoreCase: true, out CabinClass parsedCabinClass))
        {
            return ApiResponse<SearchFlightsResponse>.Failure(
                StatusCodes.Status400BadRequest,
                $"Invalid cabin class '{cabinClass}'. Must be one of: Economy, Business, FirstClass."
            );
        }

        // ===== SEARCH FLIGHTS =====
        IEnumerable<Task<IEnumerable<ProviderFlightOffer>>> tasks = _providers.Select(provider =>
            provider.SearchFlightsAsync(originCode, destinationCode, departureDate, passengers, parsedCabinClass, cancellationToken)
        );

        IEnumerable<ProviderFlightOffer>[] results = await Task.WhenAll(tasks);

        // ===== AGGREGATE WITH PRICING =====
        List<FlightResultDto> flightResults = [];

        foreach (IEnumerable<ProviderFlightOffer> providerResults in results)
        {
            foreach (ProviderFlightOffer offer in providerResults)
            {
                IProviderPricingStrategy pricingStrategy = _pricingStrategies[offer.Provider];

                decimal perPassengerPrice = pricingStrategy.CalculatePerPassengerPrice(offer.BaseFare);
                decimal totalPrice = pricingStrategy.CalculateTotalPrice(perPassengerPrice, passengers);

                FlightDetail flightDetail = new(
                    FlightId: offer.FlightId,
                    Provider: offer.Provider,
                    FlightNumber: offer.FlightNumber,
                    OriginCode: offer.OriginCode,
                    DestinationCode: offer.DestinationCode,
                    DepartureTimeUtc: offer.DepartureTimeUtc,
                    ArrivalTimeUtc: offer.ArrivalTimeUtc,
                    DurationMinutes: offer.DurationMinutes,
                    CabinClass: offer.CabinClass,
                    PerPassengerPrice: perPassengerPrice,
                    TotalPrice: totalPrice,
                    Currency: offer.Currency
                );

                flightResults.Add(flightDetail.ToFlightResultDto());
            }
        }

        // ===== RETURN RESPONSE =====
        SearchFlightsResponse response = new(Currency: "USD", Results: flightResults);

        return ApiResponse<SearchFlightsResponse>.Success(
            response,
            StatusCodes.Status200OK,
            "Flights fetched successfully"
        );
    }

    /// <summary>
    /// Retrieves a specific flight by ID with pricing applied.
    /// Returns ApiResponse with success or error details.
    /// </summary>
    public async Task<ApiResponse<FlightResultDto>> GetFlightByIdAsync(string flightId, int passengers, CancellationToken cancellationToken = default)
    {
        foreach (IFlightProvider provider in _providers)
        {
            ProviderFlightOffer? offer = await provider.GetFlightByIdAsync(flightId, cancellationToken);

            if (offer is not null)
            {
                IProviderPricingStrategy pricingStrategy = _pricingStrategies[offer.Provider];

                decimal perPassengerPrice = pricingStrategy.CalculatePerPassengerPrice(offer.BaseFare);
                decimal totalPrice = pricingStrategy.CalculateTotalPrice(perPassengerPrice, passengers);

                FlightDetail flightDetail = new(
                    FlightId: offer.FlightId,
                    Provider: offer.Provider,
                    FlightNumber: offer.FlightNumber,
                    OriginCode: offer.OriginCode,
                    DestinationCode: offer.DestinationCode,
                    DepartureTimeUtc: offer.DepartureTimeUtc,
                    ArrivalTimeUtc: offer.ArrivalTimeUtc,
                    DurationMinutes: offer.DurationMinutes,
                    CabinClass: offer.CabinClass,
                    PerPassengerPrice: perPassengerPrice,
                    TotalPrice: totalPrice,
                    Currency: offer.Currency
                );

                return ApiResponse<FlightResultDto>.Success(flightDetail.ToFlightResultDto());
            }
        }

        return ApiResponse<FlightResultDto>.Failure(
            StatusCodes.Status404NotFound,
            $"Flight with ID '{flightId}' not found."
        );
    }

    /// <summary>
    /// Validates all search request fields.
    /// Returns error message if validation fails, null if valid.
    /// </summary>
    private static string? ValidateSearchRequestFields(string originCode, string destinationCode, string departureDate, int passengers, string cabinClass)
    {
        if (string.IsNullOrWhiteSpace(originCode))
        {
            return "Origin code is required.";
        }

        if (string.IsNullOrWhiteSpace(destinationCode))
        {
            return "Destination code is required.";
        }

        if (originCode.Equals(destinationCode, StringComparison.OrdinalIgnoreCase))
        {
            return "Destination must differ from origin.";
        }

        if (string.IsNullOrWhiteSpace(departureDate))
        {
            return "Departure date is required.";
        }

        if (passengers < 1 || passengers > 9)
        {
            return "Passengers must be between 1 and 9.";
        }

        if (string.IsNullOrWhiteSpace(cabinClass))
        {
            return "Cabin class is required.";
        }

        return null;
    }
}
