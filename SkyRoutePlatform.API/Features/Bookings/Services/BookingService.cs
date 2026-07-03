using SkyRoutePlatform.API.Features.Bookings.Contracts;
using SkyRoutePlatform.API.Features.Bookings.Repositories;
using SkyRoutePlatform.API.Features.Flights.Contracts;
using SkyRoutePlatform.API.Features.Flights.Services;
using SkyRoutePlatform.API.Shared.Models;
using SkyRoutePlatform.API.Shared.Responses;
using SkyRoutePlatform.API.Shared.Validation;

namespace SkyRoutePlatform.API.Features.Bookings.Services;

/// <summary>
/// Service for orchestrating booking creation and validation.
/// Handles all validation, flight resolution, document validation, and booking persistence.
/// Returns all results wrapped in ApiResponse<T> for consistent error handling.
/// </summary>
public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightSearchService _flightSearchService;
    private readonly IDocumentValidator _documentValidator;

    /// <summary>
    /// Creates a new instance of BookingService.
    /// </summary>
    public BookingService(IBookingRepository bookingRepository, IFlightSearchService flightSearchService, IDocumentValidator documentValidator)
    {
        _bookingRepository = bookingRepository;
        _flightSearchService = flightSearchService;
        _documentValidator = documentValidator;
    }

    /// <summary>
    /// Creates a new booking with full validation.
    /// Returns ApiResponse with success or error details.
    /// </summary>
    public async Task<ApiResponse<CreateBookingResponse>> CreateBookingAsync(string flightId, string cabinClass, int passengers, IReadOnlyList<Contracts.PassengerDto> passengersDetails, CancellationToken cancellationToken = default)
    {
        // ===== FIELD VALIDATION =====
        if (string.IsNullOrWhiteSpace(flightId))
        {
            return ApiResponse<CreateBookingResponse>.Failure(StatusCodes.Status400BadRequest, "Flight ID is required.");
        }

        if (string.IsNullOrWhiteSpace(cabinClass))
        {
            return ApiResponse<CreateBookingResponse>.Failure(StatusCodes.Status400BadRequest, "Cabin class is required.");
        }

        if (passengers < 1 || passengers > 9)
        {
            return ApiResponse<CreateBookingResponse>.Failure(StatusCodes.Status400BadRequest, "Passengers must be between 1 and 9.");
        }

        if (passengersDetails is null || passengersDetails.Count != passengers)
        {
            return ApiResponse<CreateBookingResponse>.Failure(StatusCodes.Status400BadRequest, "Passengers details must be provided for each passenger.");
        }

        // basic per-passenger checks
        for (int i = 0; i < passengersDetails.Count; i++)
        {
            var p = passengersDetails[i];
            if (string.IsNullOrWhiteSpace(p.FullName))
            {
                return ApiResponse<CreateBookingResponse>.Failure(StatusCodes.Status400BadRequest, $"Full name is required for passenger {i + 1}.");
            }

            if (string.IsNullOrWhiteSpace(p.DocumentType))
            {
                return ApiResponse<CreateBookingResponse>.Failure(StatusCodes.Status400BadRequest, $"Document type is required for passenger {i + 1}.");
            }

            if (string.IsNullOrWhiteSpace(p.DocumentNumber))
            {
                return ApiResponse<CreateBookingResponse>.Failure(StatusCodes.Status400BadRequest, $"Document number is required for passenger {i + 1}.");
            }
        }

        // primary passenger (first) must have a valid email
        var primary = passengersDetails[0];
        if (string.IsNullOrWhiteSpace(primary.Email) || !primary.Email.Contains("@"))
        {
            return ApiResponse<CreateBookingResponse>.Failure(StatusCodes.Status400BadRequest, "Primary passenger must have a valid email address.");
        }

        // ===== ENUM PARSING =====
        if (!Enum.TryParse<CabinClass>(cabinClass, ignoreCase: true, out CabinClass parsedCabinClass))
        {
            return ApiResponse<CreateBookingResponse>.Failure(
                StatusCodes.Status400BadRequest,
                $"Invalid cabin class '{cabinClass}'. Must be one of: Economy, Business, FirstClass."
            );
        }

        // parse primary passenger document type
        if (!Enum.TryParse<DocumentType>(primary.DocumentType, ignoreCase: true, out DocumentType parsedDocumentType))
        {
            return ApiResponse<CreateBookingResponse>.Failure(
                StatusCodes.Status400BadRequest,
                $"Invalid document type '{primary.DocumentType}' for primary passenger. Must be one of: PassportNumber, NationalId."
            );
        }

        // ===== BUSINESS LOGIC VALIDATION =====

        // Resolve the selected flight with pricing applied
        ApiResponse<FlightResultDto> flightResponse = await _flightSearchService.GetFlightByIdAsync(flightId, passengers, cancellationToken);
        if (!flightResponse.IsSuccess)
        {
            return ApiResponse<CreateBookingResponse>.Failure(
                flightResponse.StatusCode,
                flightResponse.Message,
                flightResponse.Details
            );
        }

        FlightResultDto flightDto = flightResponse.Data!;

        // Validate cabin class matches
        if (!Enum.TryParse<CabinClass>(flightDto.CabinClass, ignoreCase: true, out CabinClass flightCabinClass))
        {
            return ApiResponse<CreateBookingResponse>.Failure(
                StatusCodes.Status422UnprocessableEntity,
                $"Invalid cabin class in flight data: {flightDto.CabinClass}"
            );
        }

        if (flightCabinClass != parsedCabinClass)
        {
            return ApiResponse<CreateBookingResponse>.Failure(
                StatusCodes.Status422UnprocessableEntity,
                $"Cabin class mismatch. Flight offers {flightCabinClass}, but booking requested {parsedCabinClass}."
            );
        }

        // Validate primary passenger document for the route
        string? documentValidationError = _documentValidator.ValidateDocumentForRoute(
            parsedDocumentType,
            primary.DocumentNumber,
            flightDto.OriginCode,
            flightDto.DestinationCode
        );

        if (documentValidationError is not null)
        {
            return ApiResponse<CreateBookingResponse>.Failure(
                StatusCodes.Status422UnprocessableEntity,
                documentValidationError
            );
        }

        // ===== CREATE AND PERSIST BOOKING =====
        string bookingReference = GenerateBookingReference();

        var passengerModels = passengersDetails.Select(p => new Passenger(
            FullName: p.FullName,
            Email: p.Email,
            DocumentType: Enum.Parse<DocumentType>(p.DocumentType, ignoreCase: true),
            DocumentNumber: p.DocumentNumber
        )).ToList().AsReadOnly();

        Booking booking = new(
            BookingReference: bookingReference,
            SelectedFlight: new FlightDetail(
                FlightId: flightDto.FlightId,
                Provider: Enum.Parse<ProviderType>(flightDto.Provider, ignoreCase: true),
                FlightNumber: flightDto.FlightNumber,
                OriginCode: flightDto.OriginCode,
                DestinationCode: flightDto.DestinationCode,
                DepartureTimeUtc: flightDto.DepartureTimeUtc,
                ArrivalTimeUtc: flightDto.ArrivalTimeUtc,
                DurationMinutes: flightDto.DurationMinutes,
                CabinClass: parsedCabinClass,
                PerPassengerPrice: flightDto.PerPassengerPrice,
                TotalPrice: flightDto.TotalPrice,
                Currency: "USD"
            ),
            Passengers: passengers,
            FullName: primary.FullName,
            Email: primary.Email ?? string.Empty,
            DocumentType: parsedDocumentType,
            DocumentNumber: primary.DocumentNumber,
            PassengerDetails: passengerModels,
            CreatedAtUtc: DateTime.UtcNow
        );

        await _bookingRepository.CreateAsync(booking, cancellationToken);

        // Return success response
        return ApiResponse<CreateBookingResponse>.Success(
            new CreateBookingResponse(bookingReference, "Confirmed"),
            StatusCodes.Status201Created,
            "Booking created successfully"
        );
    }

    /// <summary>
    /// Retrieves a booking by its reference code with full details.
    /// Returns ApiResponse with success or error details.
    /// </summary>
    public async Task<ApiResponse<GetBookingResponse>> GetBookingByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        // ===== VALIDATION =====
        if (string.IsNullOrWhiteSpace(bookingReference))
        {
            return ApiResponse<GetBookingResponse>.Failure(
                StatusCodes.Status400BadRequest,
                "Booking reference is required and cannot be empty."
            );
        }

        // ===== RETRIEVE BOOKING =====
        Booking? booking = await _bookingRepository.GetByReferenceAsync(bookingReference, cancellationToken);

        if (booking is null)
        {
            return ApiResponse<GetBookingResponse>.Failure(
                StatusCodes.Status404NotFound,
                $"Booking with reference '{bookingReference}' not found."
            );
        }

        // ===== RETURN RESPONSE =====
        var passengerInfos = booking.PassengerDetails.Select(p => new GetBookingResponse.PassengerInfo(
            FullName: p.FullName,
            Email: p.Email,
            DocumentType: p.DocumentType.ToString(),
            DocumentNumber: p.DocumentNumber
        )).ToList().AsReadOnly();

        GetBookingResponse response = new(
            BookingReference: booking.BookingReference,
            FullName: booking.FullName,
            Email: booking.Email,
            Passengers: booking.Passengers,
            Status: "Confirmed",
            Flight: new GetBookingResponse.FlightInfo(
                FlightId: booking.SelectedFlight.FlightId,
                FlightNumber: booking.SelectedFlight.FlightNumber,
                Provider: booking.SelectedFlight.Provider.ToString(),
                DepartureTimeUtc: booking.SelectedFlight.DepartureTimeUtc,
                ArrivalTimeUtc: booking.SelectedFlight.ArrivalTimeUtc,
                OriginCode: booking.SelectedFlight.OriginCode,
                DestinationCode: booking.SelectedFlight.DestinationCode,
                CabinClass: booking.SelectedFlight.CabinClass.ToString()
            ),
            TotalPrice: booking.SelectedFlight.TotalPrice,
            Currency: booking.SelectedFlight.Currency,
            PassengerDetails: passengerInfos
        );

        return ApiResponse<GetBookingResponse>.Success(response);
    }

    /// <summary>
    /// Validates all booking request fields.
    /// Returns error message if validation fails, null if valid.
    /// </summary>
    private static string? ValidateBookingRequestFields(string flightId, string cabinClass, int passengers, string fullName, string email, string documentType, string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(flightId))
        {
            return "Flight ID is required.";
        }

        if (string.IsNullOrWhiteSpace(cabinClass))
        {
            return "Cabin class is required.";
        }

        if (passengers < 1 || passengers > 9)
        {
            return "Passengers must be between 1 and 9.";
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "Full name is required.";
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            return "Valid email address is required.";
        }

        if (string.IsNullOrWhiteSpace(documentType))
        {
            return "Document type is required.";
        }

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return "Document number is required.";
        }

        return null;
    }

    /// <summary>
    /// Generates a unique booking reference code (SKY-XXXXXX format).
    /// </summary>
    private static string GenerateBookingReference()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        Random random = new();

        string code = new([.. Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)])]);

        return $"SKY-{code}";
    }
}
