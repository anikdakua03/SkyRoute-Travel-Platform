namespace SkyRoutePlatform.API.Features.Bookings.Contracts;

using SkyRoutePlatform.API.Shared.Models;

/// <summary>
/// Request contract for creating a booking.
/// </summary>
public sealed record CreateBookingRequest(string FlightId, string CabinClass, int Passengers, string FullName, string Email, string DocumentType, string DocumentNumber)
{
    /// <summary>
    /// Converts the string cabin class to the enum value.
    /// </summary>
    public CabinClass GetCabinClass()
    {
        return Enum.Parse<CabinClass>(CabinClass, ignoreCase: true);
    }

    /// <summary>
    /// Converts the string document type to the enum value.
    /// </summary>
    public DocumentType GetDocumentType()
    {
        return Enum.Parse<DocumentType>(DocumentType, ignoreCase: true);
    }
}
