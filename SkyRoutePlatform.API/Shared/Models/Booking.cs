namespace SkyRoutePlatform.API.Shared.Models;

/// <summary>
/// Represents a booking confirmation for passengers on a selected flight.
/// </summary>
public sealed record Passenger(string FullName, string? Email, DocumentType DocumentType, string DocumentNumber);

public sealed record Booking(string BookingReference, FlightDetail SelectedFlight, int Passengers, string FullName, string Email, DocumentType DocumentType, string DocumentNumber, IReadOnlyList<Passenger> PassengerDetails, DateTime CreatedAtUtc);