namespace SkyRoutePlatform.API.Features.Bookings.Contracts;

/// <summary>
/// Passenger data transfer object for booking requests.
/// </summary>
public sealed record PassengerDto(string FullName, string? Email, string DocumentType, string DocumentNumber);
