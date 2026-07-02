using Microsoft.AspNetCore.Http;
using Moq;
using Xunit.Abstractions;
using SkyRoutePlatform.API.Features.Bookings.Contracts;
using SkyRoutePlatform.API.Features.Bookings.Repositories;
using SkyRoutePlatform.API.Features.Bookings.Services;
using SkyRoutePlatform.API.Features.Flights.Contracts;
using SkyRoutePlatform.API.Features.Flights.Services;
using SkyRoutePlatform.API.Shared.Models;
using SkyRoutePlatform.API.Shared.Responses;
using SkyRoutePlatform.API.Shared.Validation;

namespace SkyRoutePlatform.Tests.Features.Bookings.Services;

/// <summary>
/// Tests for BookingService covering valid and invalid booking scenarios.
/// </summary>
public sealed class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockRepository = new();
    private readonly Mock<IFlightSearchService> _mockFlightSearchService = new();
    private readonly Mock<IDocumentValidator> _mockDocumentValidator = new();
    private readonly BookingService _service;
    private readonly ITestOutputHelper _output;

    public BookingServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _service = new BookingService(_mockRepository.Object, _mockFlightSearchService.Object, _mockDocumentValidator.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        _output.WriteLine("[TEST] CreateBookingAsync with valid booking request");
        _output.WriteLine("[TESTING] Service should validate all fields and create booking with status 201");
        _output.WriteLine("[EXPECTED] Returns success response with SKY- prefixed reference and Confirmed status");

        // Arrange
        _mockFlightSearchService
            .Setup(x => x.GetFlightByIdAsync("FL001", 2, default))
            .ReturnsAsync(ApiResponse<FlightResultDto>.Success(CreateTestFlightDto()));

        _mockDocumentValidator
            .Setup(x => x.ValidateDocumentForRoute(DocumentType.PassportNumber, "ABC123456", "JFK", "LAX"))
            .Returns((string?)null);

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Booking>(), default))
            .ReturnsAsync(It.IsAny<Booking>());

        // Act
        ApiResponse<CreateBookingResponse> result = await _service.CreateBookingAsync(
            "FL001",
            "Economy",
            2,
            "John Doe",
            "john@example.com",
            "PassportNumber",
            "ABC123456"
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.StartsWith("SKY-", result.Data.BookingReference);
        Assert.Equal("Confirmed", result.Data.Status);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Booking>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_WithInvalidFlightId_ReturnsBadRequest()
    {
        _output.WriteLine("[TEST] CreateBookingAsync with empty flight ID");
        _output.WriteLine("[TESTING] Service should reject empty flight IDs during validation");
        _output.WriteLine("[EXPECTED] Returns 400 BadRequest with flight ID required message");

        // Arrange
        // Empty flight ID

        // Act
        ApiResponse<CreateBookingResponse> result = await _service.CreateBookingAsync(
            "",
            "Economy",
            2,
            "John Doe",
            "john@example.com",
            "PassportNumber",
            "ABC123456"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Flight ID is required", result.Message);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Booking>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateBookingAsync_WithInvalidCabinClass_ReturnsBadRequest()
    {
        _output.WriteLine("[TEST] CreateBookingAsync with invalid cabin class");
        _output.WriteLine("[TESTING] Service should validate cabin class enum values (Economy, Business, FirstClass)");
        _output.WriteLine("[EXPECTED] Returns 400 BadRequest with invalid cabin class message");

        // Arrange
        // Invalid cabin class

        // Act
        ApiResponse<CreateBookingResponse> result = await _service.CreateBookingAsync(
            "FL001",
            "InvalidClass",
            2,
            "John Doe",
            "john@example.com",
            "PassportNumber",
            "ABC123456"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Invalid cabin class", result.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_WithCabinClassMismatch_ReturnsUnprocessableEntity()
    {
        _output.WriteLine("[TEST] CreateBookingAsync with cabin class not available on flight");
        _output.WriteLine("[TESTING] Service should validate requested cabin class matches flight offerings");
        _output.WriteLine("[EXPECTED] Returns 422 UnprocessableEntity with cabin class mismatch message");

        // Arrange
        FlightResultDto flightDto = CreateTestFlightDto();
        _mockFlightSearchService
            .Setup(x => x.GetFlightByIdAsync("FL001", 2, default))
            .ReturnsAsync(ApiResponse<FlightResultDto>.Success(flightDto));

        _mockDocumentValidator
            .Setup(x => x.ValidateDocumentForRoute(DocumentType.PassportNumber, "ABC123456", "JFK", "LAX"))
            .Returns((string?)null);

        // Requesting Business but flight only has Economy
        // Act
        ApiResponse<CreateBookingResponse> result = await _service.CreateBookingAsync(
            "FL001",
            "Business",
            2,
            "John Doe",
            "john@example.com",
            "PassportNumber",
            "ABC123456"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        Assert.Contains("Cabin class mismatch", result.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_WithInvalidDocumentForRoute_ReturnsUnprocessableEntity()
    {
        _output.WriteLine("[TEST] CreateBookingAsync with invalid document type for route");
        _output.WriteLine("[TESTING] Service should validate document type is acceptable for origin/destination route");
        _output.WriteLine("[EXPECTED] Returns 422 UnprocessableEntity with document validation error");

        // Arrange
        _mockFlightSearchService
            .Setup(x => x.GetFlightByIdAsync("FL001", 2, default))
            .ReturnsAsync(ApiResponse<FlightResultDto>.Success(CreateTestFlightDto()));

        _mockDocumentValidator
            .Setup(x => x.ValidateDocumentForRoute(DocumentType.PassportNumber, "ABC123456", "JFK", "LAX"))
            .Returns("Passport not valid for this route");

        // Act
        ApiResponse<CreateBookingResponse> result = await _service.CreateBookingAsync(
            "FL001",
            "Economy",
            2,
            "John Doe",
            "john@example.com",
            "PassportNumber",
            "ABC123456"
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        Assert.Contains("Passport not valid for this route", result.Message);
    }

    [Fact]
    public async Task GetBookingByReferenceAsync_WithValidReference_ReturnsSuccessResponse()
    {
        _output.WriteLine("[TEST] GetBookingByReferenceAsync with valid booking reference");
        _output.WriteLine("[TESTING] Service should retrieve booking and map it to response with 200 status");
        _output.WriteLine("[EXPECTED] Returns success response with booking details and Confirmed status");

        // Arrange
        Booking booking = new(
            BookingReference: "SKY-TEST01",
            SelectedFlight: new FlightDetail(
                FlightId: "FL001",
                Provider: ProviderType.GlobalAir,
                FlightNumber: "GA123",
                OriginCode: "JFK",
                DestinationCode: "LAX",
                DepartureTimeUtc: DateTime.UtcNow.AddDays(1),
                ArrivalTimeUtc: DateTime.UtcNow.AddDays(1).AddHours(6),
                DurationMinutes: 360,
                CabinClass: CabinClass.Economy,
                PerPassengerPrice: 250.00m,
                TotalPrice: 500.00m,
                Currency: "USD"
            ),
            Passengers: 2,
            FullName: "John Doe",
            Email: "john@example.com",
            DocumentType: DocumentType.PassportNumber,
            DocumentNumber: "ABC123456",
            CreatedAtUtc: DateTime.UtcNow
        );

        _mockRepository
            .Setup(x => x.GetByReferenceAsync("SKY-TEST01", default))
            .ReturnsAsync(booking);

        // Act
        ApiResponse<GetBookingResponse> result = await _service.GetBookingByReferenceAsync("SKY-TEST01");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("SKY-TEST01", result.Data.BookingReference);
        Assert.Equal("John Doe", result.Data.FullName);
        Assert.Equal("Confirmed", result.Data.Status);
    }

    [Fact]
    public async Task GetBookingByReferenceAsync_WithNonExistentReference_ReturnsNotFound()
    {
        _output.WriteLine("[TEST] GetBookingByReferenceAsync with non-existent booking reference");
        _output.WriteLine("[TESTING] Service should handle missing bookings gracefully without exceptions");
        _output.WriteLine("[EXPECTED] Returns 404 NotFound with 'not found' message");

        // Arrange
        _mockRepository
            .Setup(x => x.GetByReferenceAsync("SKY-NOTFOUND", default))
            .ReturnsAsync((Booking?)null);

        // Act
        ApiResponse<GetBookingResponse> result = await _service.GetBookingByReferenceAsync("SKY-NOTFOUND");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task GetBookingByReferenceAsync_WithEmptyReference_ReturnsBadRequest()
    {
        _output.WriteLine("[TEST] GetBookingByReferenceAsync with empty booking reference");
        _output.WriteLine("[TESTING] Service should validate booking reference is not empty");
        _output.WriteLine("[EXPECTED] Returns 400 BadRequest with reference required message");

        // Act
        ApiResponse<GetBookingResponse> result = await _service.GetBookingByReferenceAsync("");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("Booking reference is required", result.Message);
    }

    private static FlightResultDto CreateTestFlightDto(string flightId = "FL001")
    {
        return new FlightResultDto(
            FlightId: flightId,
            FlightNumber: "GA123",
            Provider: "GlobalAir",
            OriginCode: "JFK",
            DestinationCode: "LAX",
            DepartureTimeUtc: DateTime.UtcNow.AddDays(1),
            ArrivalTimeUtc: DateTime.UtcNow.AddDays(1).AddHours(6),
            DurationMinutes: 360,
            CabinClass: "Economy",
            PerPassengerPrice: 250.00m,
            TotalPrice: 500.00m
        );
    }
}
