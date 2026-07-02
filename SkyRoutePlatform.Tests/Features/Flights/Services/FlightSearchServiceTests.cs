using Microsoft.AspNetCore.Http;
using Moq;
using SkyRoutePlatform.API.Features.Flights.Contracts;
using SkyRoutePlatform.API.Features.Flights.Providers;
using SkyRoutePlatform.API.Features.Flights.Services;
using SkyRoutePlatform.API.Shared.Models;
using SkyRoutePlatform.API.Shared.Responses;
using Xunit.Abstractions;

namespace SkyRoutePlatform.Tests.Features.Flights.Services;

/// <summary>
/// Tests for FlightSearchService covering valid and invalid search scenarios.
/// </summary>
public sealed class FlightSearchServiceTests
{
    private readonly FlightSearchService _service;

    private readonly Mock<IFlightProvider> _mockProvider1 = new();
    private readonly Mock<IFlightProvider> _mockProvider2 = new();
    private readonly IEnumerable<IFlightProvider> _providers;
    private readonly ITestOutputHelper _output;

    public FlightSearchServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _providers = [_mockProvider1.Object, _mockProvider2.Object];
        _service = new FlightSearchService(_providers);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithValidRequest_ReturnsFlightsFromAllProviders()
    {
        _output.WriteLine("[TEST] SearchFlightsAsync with valid search parameters");
        _output.WriteLine("[TESTING] Service should query all providers and aggregate flight results with pricing");
        _output.WriteLine("[EXPECTED] Returns 200 success with flights from multiple providers (total 2 flights)");

        // Arrange
        _mockProvider1
            .Setup(x => x.SearchFlightsAsync("JFK", "LAX", "2026-07-10", 2, CabinClass.Economy, default))
            .ReturnsAsync([CreateTestOffer("FL001", ProviderType.GlobalAir)]);

        _mockProvider2
            .Setup(x => x.SearchFlightsAsync("JFK", "LAX", "2026-07-10", 2, CabinClass.Economy, default))
            .ReturnsAsync([CreateTestOffer("FL002", ProviderType.BudgetWings)]);

        // Act
        ApiResponse<SearchFlightsResponse> result = await _service.SearchFlightsAsync(
            "JFK",
            "LAX",
            "2026-07-10",
            2,
            "Economy"
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Results.Count);
        Assert.Contains(result.Data.Results, r => r.FlightId == "FL001");
        Assert.Contains(result.Data.Results, r => r.FlightId == "FL002");
    }

    [Fact]
    public async Task SearchFlightsAsync_WithInvalidOriginCode_ReturnsBadRequest()
    {
        _output.WriteLine("[TEST] SearchFlightsAsync with empty origin code");
        _output.WriteLine("[TESTING] Service should validate origin code is not empty");
        _output.WriteLine("[EXPECTED] Returns 400 BadRequest with origin code required message");

        // Act
        ApiResponse<SearchFlightsResponse> result = await _service.SearchFlightsAsync(
            "",
            "LAX",
            "2026-07-10",
            2,
            "Economy"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Origin code is required", result.Message);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithSameOriginAndDestination_ReturnsBadRequest()
    {
        _output.WriteLine("[TEST] SearchFlightsAsync with same origin and destination");
        _output.WriteLine("[TESTING] Service should validate origin and destination are different");
        _output.WriteLine("[EXPECTED] Returns 400 BadRequest with destination/origin difference message");

        // Act
        ApiResponse<SearchFlightsResponse> result = await _service.SearchFlightsAsync(
            "JFK",
            "JFK",
            "2026-07-10",
            2,
            "Economy"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Destination must differ from origin", result.Message);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithInvalidDateFormat_ReturnsBadRequest()
    {
        _output.WriteLine("[TEST] SearchFlightsAsync with invalid date format");
        _output.WriteLine("[TESTING] Service should validate departure date is in correct format (YYYY-MM-DD)");
        _output.WriteLine("[EXPECTED] Returns 400 BadRequest with invalid date format message");

        // Act
        ApiResponse<SearchFlightsResponse> result = await _service.SearchFlightsAsync(
            "JFK",
            "LAX",
            "invalid-date",
            2,
            "Economy"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Invalid departure date format", result.Message);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithInvalidCabinClass_ReturnsBadRequest()
    {
        _output.WriteLine("[TEST] SearchFlightsAsync with invalid cabin class");
        _output.WriteLine("[TESTING] Service should validate cabin class enum values");
        _output.WriteLine("[EXPECTED] Returns 400 BadRequest with invalid cabin class message");

        // Act
        ApiResponse<SearchFlightsResponse> result = await _service.SearchFlightsAsync(
            "JFK",
            "LAX",
            "2026-07-10",
            2,
            "InvalidClass"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Invalid cabin class", result.Message);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithInvalidPassengerCount_ReturnsBadRequest()
    {
        _output.WriteLine("[TEST] SearchFlightsAsync with invalid passenger count (zero)");
        _output.WriteLine("[TESTING] Service should validate passenger count is between 1 and 9");
        _output.WriteLine("[EXPECTED] Returns 400 BadRequest with valid passenger count range message");

        // Act - Zero passengers
        ApiResponse<SearchFlightsResponse> result = await _service.SearchFlightsAsync(
            "JFK",
            "LAX",
            "2026-07-10",
            0,
            "Economy"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Passengers must be between 1 and 9", result.Message);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithPricingApplied_ReturnsAggregatedResults()
    {
        _output.WriteLine("[TEST] SearchFlightsAsync with pricing strategy applied");
        _output.WriteLine("[TESTING] Service should calculate per-passenger and total prices for results");
        _output.WriteLine("[EXPECTED] Returns flights with correct pricing calculations applied");

        // Arrange
        _mockProvider1
            .Setup(x => x.SearchFlightsAsync("JFK", "LAX", "2026-07-10", 2, CabinClass.Economy, default))
            .ReturnsAsync([CreateTestOffer("FL001", ProviderType.GlobalAir)]);

        _mockProvider2
            .Setup(x => x.SearchFlightsAsync("JFK", "LAX", "2026-07-10", 2, CabinClass.Economy, default))
            .ReturnsAsync(Array.Empty<ProviderFlightOffer>());

        // Act
        ApiResponse<SearchFlightsResponse> result = await _service.SearchFlightsAsync(
            "JFK",
            "LAX",
            "2026-07-10",
            2,
            "Economy"
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Results);
        FlightResultDto flight = result.Data.Results.First();
        Assert.Equal("FL001", flight.FlightId);

        Assert.True(flight.TotalPrice > 0);
        Assert.True(flight.PerPassengerPrice > 0);
    }

    [Fact]
    public async Task GetFlightByIdAsync_WithExistingFlightId_ReturnsFlightWithPricing()
    {
        _output.WriteLine("[TEST] GetFlightByIdAsync with existing flight ID");
        _output.WriteLine("[TESTING] Service should retrieve flight and apply pricing for given passenger count");
        _output.WriteLine("[EXPECTED] Returns 200 success with flight details and calculated pricing");

        // Arrange
        _mockProvider1
            .Setup(x => x.GetFlightByIdAsync("FL001", default))
            .ReturnsAsync(CreateTestOffer("FL001", ProviderType.GlobalAir));

        _mockProvider2
            .Setup(x => x.GetFlightByIdAsync("FL001", default))
            .ReturnsAsync((ProviderFlightOffer?)null);

        // Act
        ApiResponse<FlightResultDto> result = await _service.GetFlightByIdAsync("FL001", 2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("FL001", result.Data.FlightId);
        Assert.Equal("GA123", result.Data.FlightNumber);
        Assert.True(result.Data.TotalPrice > 0);
    }

    [Fact]
    public async Task GetFlightByIdAsync_WithNonExistentFlightId_ReturnsNotFound()
    {
        _output.WriteLine("[TEST] GetFlightByIdAsync with non-existent flight ID");
        _output.WriteLine("[TESTING] Service should search all providers and handle when flight is not found");
        _output.WriteLine("[EXPECTED] Returns 404 NotFound with 'not found' message");

        // Arrange
        _mockProvider1
            .Setup(x => x.GetFlightByIdAsync("NONEXISTENT", default))
            .ReturnsAsync((ProviderFlightOffer?)null);

        _mockProvider2
            .Setup(x => x.GetFlightByIdAsync("NONEXISTENT", default))
            .ReturnsAsync((ProviderFlightOffer?)null);

        // Act
        ApiResponse<FlightResultDto> result = await _service.GetFlightByIdAsync("NONEXISTENT", 2);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task GetFlightByIdAsync_SearchesMultipleProviders_ReturnsFirstMatch()
    {
        _output.WriteLine("[TEST] GetFlightByIdAsync searches multiple providers sequentially");
        _output.WriteLine("[TESTING] Service should query providers in order and return first match found");
        _output.WriteLine("[EXPECTED] Returns flight from second provider (first returned null) with correct details");

        // Arrange
        _mockProvider1
            .Setup(x => x.GetFlightByIdAsync("FL001", default))
            .ReturnsAsync((ProviderFlightOffer?)null);

        _mockProvider2
            .Setup(x => x.GetFlightByIdAsync("FL001", default))
            .ReturnsAsync(CreateTestOffer("FL001", ProviderType.BudgetWings));

        // Act
        ApiResponse<FlightResultDto> result = await _service.GetFlightByIdAsync("FL001", 2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("FL001", result.Data.FlightId);
        Assert.Equal("BudgetWings", result.Data.Provider);
    }

    private static ProviderFlightOffer CreateTestOffer(string flightId = "FL001", ProviderType provider = ProviderType.GlobalAir)
    {
        return new ProviderFlightOffer(
            FlightId: flightId,
            FlightNumber: "GA123",
            Provider: provider,
            OriginCode: "JFK",
            DestinationCode: "LAX",
            DepartureTimeUtc: DateTime.UtcNow.AddDays(1),
            ArrivalTimeUtc: DateTime.UtcNow.AddDays(1).AddHours(6),
            DurationMinutes: 360,
            CabinClass: CabinClass.Economy,
            BaseFare: 200.00m,
            Currency: "USD"
        );
    }
}
