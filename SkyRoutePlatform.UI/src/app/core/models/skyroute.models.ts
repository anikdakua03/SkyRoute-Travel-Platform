export type CabinClass = 'Economy' | 'Business' | 'FirstClass';

export type DocumentType = 'PassportNumber' | 'NationalId';

export interface AirportOption {
    code: string;
    city: string;
    countryCode: string;
    countryName: string;
}

export interface SearchFlightsRequest {
    originCode: string;
    destinationCode: string;
    departureDate: string;
    passengers: number;
    cabinClass: CabinClass;
}

export interface FlightResult {
    flightId: string;
    provider: string;
    flightNumber: string;
    originCode: string;
    destinationCode: string;
    departureTimeUtc: string;
    arrivalTimeUtc: string;
    durationMinutes: number;
    cabinClass: string;
    perPassengerPrice: number;
    totalPrice: number;
}

export interface SearchFlightsResponse {
    currency: string;
    results: FlightResult[];
}

export interface CreateBookingRequest {
    flightId: string;
    cabinClass: string;
    passengers: number;
    passengersDetails: Array<{
        fullName: string;
        email?: string | null;
        documentType: DocumentType;
        documentNumber: string;
    }>;
}

export interface CreateBookingResponse {
    bookingReference: string;
    status: string;
}

export interface GetBookingResponse {
    bookingReference: string;
    fullName: string;
    email: string;
    passengers: number;
    status: string;
    flight: GetBookingFlightInfo;
    totalPrice: number;
    currency: string;
    passengerDetails: GetBookingPassengerInfo[];
}

export interface GetBookingPassengerInfo {
    fullName: string;
    email?: string | null;
    documentType: string;
    documentNumber: string;
}

export interface GetBookingFlightInfo {
    flightId: string;
    flightNumber: string;
    provider: string;
    departureTimeUtc: string;
    arrivalTimeUtc: string;
    originCode: string;
    destinationCode: string;
    cabinClass: string;
}

export interface ApiResponse<T> {
    data: T | null;
    statusCode: number;
    message: string;
    details: Record<string, string | null> | null;
    timestamp: string;
}

export interface ProblemDetailsResponse {
    title?: string;
    detail?: string;
    status?: number;
    type?: string;
    instance?: string;
    errors?: Record<string, string[]>;
}

export interface BookingContext {
    passengers: number;
    departureDate: string;
}

export type SortOption =
    | 'price-asc'
    | 'price-desc'
    | 'duration-asc'
    | 'departure-asc';
