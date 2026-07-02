using Xunit;
using Xunit.Abstractions;
using SkyRoutePlatform.API.Features.Bookings.Repositories;
using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.Tests.Features.Bookings.Repositories;

/// <summary>
/// Tests for InMemoryBookingRepository covering valid and invalid scenarios.
/// </summary>
public sealed class InMemoryBookingRepositoryTests
{
    private readonly InMemoryBookingRepository _repository = new();
    private readonly ITestOutputHelper _output;

    public InMemoryBookingRepositoryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task CreateAsync_WithValidBooking_ReturnsBooking()
    {
        _output.WriteLine("[TEST] CreateAsync with valid booking");
        _output.WriteLine("[TESTING] Repository should store a new booking with valid data");
        _output.WriteLine("[EXPECTED] Returns created booking with correct reference, name, and passenger count");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();
        Booking testBooking = CreateTestBooking("SKY-VALID01");

        // Act
        Booking result = await _repository.CreateAsync(testBooking);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SKY-VALID01", result.BookingReference);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal(2, result.Passengers);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateReference_ThrowsInvalidOperationException()
    {
        _output.WriteLine("[TEST] CreateAsync with duplicate booking reference");
        _output.WriteLine("[TESTING] Repository should prevent storing bookings with duplicate references");
        _output.WriteLine("[EXPECTED] Throws InvalidOperationException when attempting to create booking with existing reference");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();
        Booking booking1 = CreateTestBooking("SKY-DUP001");
        Booking booking2 = CreateTestBooking("SKY-DUP001"); // Same reference

        await _repository.CreateAsync(booking1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.CreateAsync(booking2)
        );
    }

    [Fact]
    public async Task GetByReferenceAsync_WithExistingReference_ReturnsBooking()
    {
        _output.WriteLine("[TEST] GetByReferenceAsync with existing booking reference");
        _output.WriteLine("[TESTING] Repository should retrieve a stored booking by its reference code");
        _output.WriteLine("[EXPECTED] Returns the correct booking with matching email and reference");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();
        Booking testBooking = CreateTestBooking("SKY-GET001");
        await _repository.CreateAsync(testBooking);

        // Act
        Booking? result = await _repository.GetByReferenceAsync("SKY-GET001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SKY-GET001", result.BookingReference);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task GetByReferenceAsync_WithNonExistentReference_ReturnsNull()
    {
        _output.WriteLine("[TEST] GetByReferenceAsync with non-existent booking reference");
        _output.WriteLine("[TESTING] Repository should handle gracefully when booking reference does not exist");
        _output.WriteLine("[EXPECTED] Returns null without throwing exception");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();

        // Act
        Booking? result = await _repository.GetByReferenceAsync("SKY-NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByReferenceAsync_WithMultipleBookings_ReturnsCorrectOne()
    {
        _output.WriteLine("[TEST] GetByReferenceAsync with multiple bookings stored");
        _output.WriteLine("[TESTING] Repository should correctly retrieve specific booking among many stored bookings");
        _output.WriteLine("[EXPECTED] Returns the correct booking (SKY-MULTI02) with matching reference and passenger data");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();
        Booking booking1 = CreateTestBooking("SKY-MULTI01");
        Booking booking2 = CreateTestBooking("SKY-MULTI02");
        Booking booking3 = CreateTestBooking("SKY-MULTI03");

        await _repository.CreateAsync(booking1);
        await _repository.CreateAsync(booking2);
        await _repository.CreateAsync(booking3);

        // Act
        Booking? result = await _repository.GetByReferenceAsync("SKY-MULTI02");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SKY-MULTI02", result.BookingReference);
        Assert.Equal(2, result.Passengers);
    }

    [Fact]
    public async Task ClearAllBookings_RemovesAllStoredBookings()
    {
        _output.WriteLine("[TEST] ClearAllBookings removes all stored bookings");
        _output.WriteLine("[TESTING] Repository should remove all bookings from storage when clear is called");
        _output.WriteLine("[EXPECTED] GetAllBookings returns empty collection after clear operation");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();
        Booking booking1 = CreateTestBooking("SKY-CLEAR01");
        Booking booking2 = CreateTestBooking("SKY-CLEAR02");

        await _repository.CreateAsync(booking1);
        await _repository.CreateAsync(booking2);

        Assert.Equal(2, InMemoryBookingRepository.GetAllBookings().Count);

        // Act
        InMemoryBookingRepository.ClearAllBookings();

        // Assert
        Assert.Empty(InMemoryBookingRepository.GetAllBookings());
    }

    [Fact]
    public async Task GetAllBookings_WithMultipleBookings_ReturnsAllBookingsReadOnly()
    {
        _output.WriteLine("[TEST] GetAllBookings with multiple stored bookings");
        _output.WriteLine("[TESTING] Repository should retrieve all stored bookings in a read-only collection");
        _output.WriteLine("[EXPECTED] Returns all 3 bookings with correct references and read-only collection type");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();
        Booking booking1 = CreateTestBooking("SKY-ALL01");
        Booking booking2 = CreateTestBooking("SKY-ALL02");
        Booking booking3 = CreateTestBooking("SKY-ALL03");

        await _repository.CreateAsync(booking1);
        await _repository.CreateAsync(booking2);
        await _repository.CreateAsync(booking3);

        // Act
        IReadOnlyCollection<Booking> result = InMemoryBookingRepository.GetAllBookings();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, b => b.BookingReference == "SKY-ALL01");
        Assert.Contains(result, b => b.BookingReference == "SKY-ALL02");
        Assert.Contains(result, b => b.BookingReference == "SKY-ALL03");
    }

    [Fact]
    public void GetAllBookings_WhenEmpty_ReturnsEmptyCollection()
    {
        _output.WriteLine("[TEST] GetAllBookings when repository is empty");
        _output.WriteLine("[TESTING] Repository should handle empty state gracefully");
        _output.WriteLine("[EXPECTED] Returns empty read-only collection without errors");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();

        // Act
        IReadOnlyCollection<Booking> result = InMemoryBookingRepository.GetAllBookings();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_PreservesBookingData_WithComplexDetails()
    {
        _output.WriteLine("[TEST] CreateAsync preserves complex booking data with multiple providers and cabin classes");
        _output.WriteLine("[TESTING] Repository should correctly store and retrieve booking with detailed information");
        _output.WriteLine("[EXPECTED] All booking fields preserved including document info, multiple passengers, Business cabin class, and international route");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();
        Booking complexBooking = new(
            BookingReference: "SKY-COMPLEX",
            SelectedFlight: new FlightDetail(
                FlightId: "FL-COMPLEX-001",
                Provider: ProviderType.BudgetWings,
                FlightNumber: "BW9876",
                OriginCode: "LHR",
                DestinationCode: "CDG",
                DepartureTimeUtc: DateTime.UtcNow.AddDays(7),
                ArrivalTimeUtc: DateTime.UtcNow.AddDays(7).AddHours(3),
                DurationMinutes: 180,
                CabinClass: CabinClass.Business,
                PerPassengerPrice: 1500.00m,
                TotalPrice: 4500.00m,
                Currency: "EUR"
            ),
            Passengers: 3,
            FullName: "Jane Smith Johnson",
            Email: "jane.smith@company.com",
            DocumentType: DocumentType.NationalId,
            DocumentNumber: "ID-987654321",
            CreatedAtUtc: DateTime.UtcNow
        );

        // Act
        Booking created = await _repository.CreateAsync(complexBooking);
        Booking? retrieved = await _repository.GetByReferenceAsync("SKY-COMPLEX");

        // Assert
        Assert.NotNull(created);
        Assert.NotNull(retrieved);
        Assert.Equal("SKY-COMPLEX", retrieved.BookingReference);
        Assert.Equal(3, retrieved.Passengers);
        Assert.Equal("jane.smith@company.com", retrieved.Email);
        Assert.Equal(DocumentType.NationalId, retrieved.DocumentType);
        Assert.Equal("ID-987654321", retrieved.DocumentNumber);
        Assert.Equal(CabinClass.Business, retrieved.SelectedFlight.CabinClass);
        Assert.Equal(ProviderType.BudgetWings, retrieved.SelectedFlight.Provider);
        Assert.Equal("LHR", retrieved.SelectedFlight.OriginCode);
        Assert.Equal("CDG", retrieved.SelectedFlight.DestinationCode);
    }

    [Fact]
    public async Task CreateAsync_SequentialBookings_MaintainsIndependentState()
    {
        _output.WriteLine("[TEST] CreateAsync maintains independent state for sequential bookings");
        _output.WriteLine("[TESTING] Repository should store multiple bookings with different cabin classes and routes without interference");
        _output.WriteLine("[EXPECTED] Each booking maintains its own passenger names, cabin classes, and routes independently");

        // Arrange
        InMemoryBookingRepository.ClearAllBookings();

        Booking booking1 = CreateTestBooking("SKY-SEQ01");
        Booking booking2 = new(
            BookingReference: "SKY-SEQ02",
            SelectedFlight: new FlightDetail(
                FlightId: "FL002",
                Provider: ProviderType.BudgetWings,
                FlightNumber: "BW999",
                OriginCode: "ORD",
                DestinationCode: "SFO",
                DepartureTimeUtc: DateTime.UtcNow.AddDays(2),
                ArrivalTimeUtc: DateTime.UtcNow.AddDays(2).AddHours(4),
                DurationMinutes: 240,
                CabinClass: CabinClass.FirstClass,
                PerPassengerPrice: 5000.00m,
                TotalPrice: 10000.00m,
                Currency: "USD"
            ),
            Passengers: 2,
            FullName: "Alice Wonder",
            Email: "alice@example.com",
            DocumentType: DocumentType.PassportNumber,
            DocumentNumber: "PP999888777",
            CreatedAtUtc: DateTime.UtcNow
        );

        // Act
        await _repository.CreateAsync(booking1);
        await _repository.CreateAsync(booking2);

        Booking? result1 = await _repository.GetByReferenceAsync("SKY-SEQ01");
        Booking? result2 = await _repository.GetByReferenceAsync("SKY-SEQ02");

        // Assert - Verify each booking maintains its own state independently
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal("John Doe", result1.FullName);
        Assert.Equal("Alice Wonder", result2.FullName);
        Assert.Equal(CabinClass.Economy, result1.SelectedFlight.CabinClass);
        Assert.Equal(CabinClass.FirstClass, result2.SelectedFlight.CabinClass);
        Assert.Equal("JFK", result1.SelectedFlight.OriginCode);
        Assert.Equal("ORD", result2.SelectedFlight.OriginCode);
    }

    private static Booking CreateTestBooking(string reference = "SKY-ABC123")
    {
        return new Booking(
            BookingReference: reference,
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
    }
}
