namespace SkyRoutePlatform.API.Shared.Models;

/// <summary>
/// Represents a booking confirmation for a passenger on a selected flight.
/// </summary>
public sealed record Booking(string BookingReference, FlightDetail SelectedFlight, int Passengers, string FullName, string Email, DocumentType DocumentType, string DocumentNumber, DateTime CreatedAtUtc);