namespace SkyRoutePlatform.API.Features.Bookings.Contracts;

/// <summary>
/// Response model for retrieving booking details.
/// </summary>
/// <param name="BookingReference">The unique booking reference code</param>
/// <param name="FullName">Full name of the passenger</param>
/// <param name="Email">Email address</param>
/// <param name="Passengers">Number of passengers</param>
/// <param name="Status">Booking status (e.g., "Confirmed")</param>
/// <param name="Flight">Flight information for the booking</param>
/// <param name="TotalPrice">Total price for all passengers</param>
/// <param name="Currency">Currency code (e.g., "USD")</param>
public sealed record GetBookingResponse(string BookingReference, string FullName, string Email, int Passengers, string Status, GetBookingResponse.FlightInfo Flight, decimal TotalPrice, string Currency, IReadOnlyList<GetBookingResponse.PassengerInfo> PassengerDetails)
{
    /// <summary>
    /// Flight information included in the booking.
    /// </summary>
    /// <param name="FlightId">Unique flight identifier</param>
    /// <param name="FlightNumber">Airline flight number</param>
    /// <param name="Provider">Flight provider name</param>
    /// <param name="DepartureTimeUtc">Departure time in UTC</param>
    /// <param name="ArrivalTimeUtc">Arrival time in UTC</param>
    /// <param name="OriginCode">Origin airport code</param>
    /// <param name="DestinationCode">Destination airport code</param>
    /// <param name="CabinClass">Cabin class for the booking</param>
    public sealed record FlightInfo(string FlightId, string FlightNumber, string Provider, DateTime DepartureTimeUtc, DateTime ArrivalTimeUtc, string OriginCode, string DestinationCode, string CabinClass);

    /// <summary>
    /// Passenger information included in the booking.
    /// </summary>
    public sealed record PassengerInfo(string FullName, string? Email, string DocumentType, string DocumentNumber);
}
