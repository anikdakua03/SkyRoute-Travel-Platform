# Specification

## 1. Problem Statement

### 1.1 Business Context
SkyRoute is a travel aggregator that must let users search, compare, and book flights while integrating multiple airline providers with different pricing rules. The immediate business need is a production-quality Flight Search and Booking module that can run locally for demonstration and interview walkthrough, while being architected to support additional providers over time.

Two providers must be supported in this iteration using backend mocks:
- GlobalAir: final per-passenger fare = base fare + 15% fuel surcharge, rounded to 2 decimals.
- BudgetWings: final per-passenger fare = base fare - 10% discount, with minimum fare floor of 29.99, discount applied only to base fare.

### 1.2 Goals
- Deliver an end-to-end flight search and booking flow using Angular frontend and .NET backend.
- Support search inputs: origin, destination, departure date, passengers (1-9), and cabin class.
- Return and display normalized flight results across providers with correct pricing semantics:
  - Total price for all passengers.
  - Secondary per-passenger price.
- Support frontend-only sorting by price, duration, and departure time without additional API calls.
- Provide booking submission with passenger details and return a booking reference code.
- Dynamically switch document label and validation behavior for domestic vs international routes.
- Keep architecture extensible for future provider onboarding.

### 1.3 Non-Goals
- Real external airline API integrations (mocked providers only).
- Cloud deployment and production infrastructure setup.
- Live airport search/autocomplete integration.
- Advanced itinerary combinations (multi-city, round-trip, multi-leg optimization).

## 2. Scope

### 2.1 In Scope
- **Search Form Inputs**: Implement origin/destination dropdowns with at least 6 airports across at least 2 countries, departure date, passenger count (1-9), and cabin class.
- **Provider Aggregation**: Query mocked GlobalAir and BudgetWings providers through backend abstractions and return normalized flight results.
- **Pricing Rules**: Apply provider-specific fare calculations correctly and expose both per-passenger and total prices.
- **Result Presentation**: Show provider, flight number, departure/arrival times, duration, cabin class, and pricing information.
- **Frontend Sorting**: Sort already-fetched results on the frontend by price ascending/descending, shortest duration, and departure time.
- **Loading and Empty States**: Show loading indicator during search and clear empty state when no results exist.
- **Booking Flow**: Allow selecting a flight, show booking summary and price breakdown, collect passenger details, and confirm booking.
- **Dynamic Document Field**: Switch label and validation between Passport Number (international) and National ID (domestic).
- **API Contracts**: Define and implement endpoints that support search, provider-agnostic pricing output, and booking confirmation.
- **README Delivery Requirement**: Document setup/run instructions, architecture decisions, and known limitations.

### 2.2 Out of Scope
- **Real Provider Connectivity**: No live calls to external airline systems in this challenge.
- **Payment Processing**: No card wallet payment gateway integration.
- **User Accounts**: No authentication, authorization, saved traveler profiles, or booking history.
- **Post-Booking Operations**: No cancellation, rescheduling, refund, or ticket management workflows.
- **Cloud/DevOps Hardening**: No mandatory cloud hosting, distributed tracing backends, or production scaling setup.

## 3. API Project Architecture (Possible, For Discussion)

### 3.1 Proposed Project Structure
Possible backend structure for SkyRoutePlatform.API:
- Features/
  - Flights/
    - Contracts/
      - SearchFlightsRequest.cs
      - SearchFlightsResponse.cs
    - Endpoints/
      - SearchFlightsEndpoint.cs
    - Services/
      - FlightSearchService.cs
    - Providers/
      - IFlightProvider.cs
      - IProviderDataSource.cs
      - JsonFileProviderDataSource.cs
      - Dtos/
        - GlobalAirFlightDto.cs
        - BudgetWingsFlightDto.cs
      - GlobalAirProviderMock.cs
      - BudgetWingsProviderMock.cs
    - Mappings/
      - GlobalAirFlightDtoExtensions.cs
      - BudgetWingsFlightDtoExtensions.cs
    - Pricing/
      - IProviderPricingStrategy.cs
      - GlobalAirPricingStrategy.cs
      - BudgetWingsPricingStrategy.cs
  - Bookings/
    - Contracts/
      - CreateBookingRequest.cs
      - CreateBookingResponse.cs
    - Endpoints/
      - CreateBookingEndpoint.cs
    - Services/
      - BookingService.cs
    - Repositories/
      - IBookingRepository.cs
      - InMemoryBookingRepository.cs
- Shared/
  - Errors/
    - ProblemDetailsFactoryExtensions.cs
    - ErrorCodeCatalog.cs
  - Validation/
    - RouteDocumentValidationService.cs
- Middlewares/
  - GlobalExceptionHandlerMiddleware.cs
- Data/
  - providers/
    - global-air-flights.json
    - budget-wings-flights.json

Possible frontend structure for Angular app (separate project/folder):
- src/app/core/
  - api/
  - models/
  - utils/
- src/app/features/flight-search/
  - flight-search-page.component.ts
  - flight-results-table.component.ts
  - sort-controls.component.ts
- src/app/features/booking/
  - booking-page.component.ts
  - booking-summary.component.ts
  - passenger-form.component.ts

### 3.2 Discussion Points
- **Pattern Choice**: Strategy pattern for provider-specific pricing and provider adapters to avoid conditional logic sprawl.
- **Extensibility**: New providers should be add-only by implementing provider and pricing strategy contracts.
- **Mock Data Source Boundary**: Provider mocks should read provider-owned JSON files via `IProviderDataSource`, then normalize to the unified result model.
- **Normalization Strategy**: Convert provider-specific DTOs to provider offers, then map into a unified `FlightDetail` model using extension methods to keep mapping logic type-safe and provider-local.
- **Deterministic Mock Coverage**: Provider mocks should return realistic deterministic results for any valid search request, while still allowing empty responses for genuinely unavailable combinations after business filtering.
- **API Style**: Minimal API or Controller-based API are both viable; choose one based on team consistency and testability preference.
- **Validation Placement**: Keep request shape validation at endpoint boundary and route-based document validation in a domain service.
- **Global Exception Handling**: Use a single global exception middleware (or framework-level exception handler) to map unhandled exceptions to RFC Problem Details consistently across all endpoints.
- **Program Pipeline Order**: Register exception handling early in the request pipeline so downstream failures are captured and returned in consistent error contracts.
- **Data Persistence**: Booking service should depend on `IBookingRepository`; use in-memory now and swap to SQL/NoSQL implementations later without endpoint contract changes.

### 3.3 UI Architecture Notes (if UI exists)
UI is required by the PRD and the framework is Angular (as specified in the PRD stack choice).

UI architecture notes:
- Keep search state local to feature page and derive sorted views in-memory.
- Preserve selected flight and search context for booking screen navigation.
- Represent document type as route-derived computed state to control both label and validator.
- Ensure clear distinction between total price and per-passenger price in all result/booking displays.

## 4. Data Models

### 4.1 Core Entities
- Airport
  - code: string (IATA-like)
  - city: string
  - countryCode: string
  - countryName: string
- FlightDetail
  - provider: enum (ProviderType: GlobalAir, BudgetWings)
  - flightNumber: string
  - originCode: string
  - destinationCode: string
  - departureTimeUtc: datetime
  - arrivalTimeUtc: datetime
  - durationMinutes: int
  - cabinClass: enum (Economy, Business, FirstClass)
  - perPassengerPrice: decimal
  - totalPrice: decimal
  - currency: string (USD)
- Booking
  - bookingReference: string
  - selectedFlight: FlightDetail snapshot
  - passengers: int
  - fullName: string
  - email: string
  - documentType: enum (PassportNumber, NationalId)
  - documentNumber: string
  - createdAtUtc: datetime

### 4.1.1 Proposed C# Model Structure (For Discussion)
```csharp
public enum CabinClass
{
  Economy,
  Business,
  FirstClass
}

public enum DocumentType
{
  PassportNumber,
  NationalId
}

public enum ProviderType
{
  GlobalAir,
  BudgetWings
}

public sealed record Airport(
  string Code,
  string City,
  string CountryCode,
  string CountryName
);

public sealed record FlightDetail(
  string FlightId,
  ProviderType Provider,
  string FlightNumber,
  string OriginCode,
  string DestinationCode,
  DateTime DepartureTimeUtc,
  DateTime ArrivalTimeUtc,
  int DurationMinutes,
  CabinClass CabinClass,
  decimal PerPassengerPrice,
  decimal TotalPrice,
  string Currency
);

public sealed record ProviderFlightOffer(
  string FlightId,
  ProviderType Provider,
  string FlightNumber,
  string OriginCode,
  string DestinationCode,
  DateTime DepartureTimeUtc,
  DateTime ArrivalTimeUtc,
  int DurationMinutes,
  CabinClass CabinClass,
  decimal BaseFare,
  string Currency
);

public sealed record Booking(
  string BookingReference,
  FlightDetail SelectedFlight,
  int Passengers,
  string FullName,
  string Email,
  DocumentType DocumentType,
  string DocumentNumber,
  DateTime CreatedAtUtc
);
```

### 4.2 DTOs and Validation Rules
- SearchFlightsRequest
  - originCode: required, must exist in configured airport set
  - destinationCode: required, must differ from origin
  - departureDate: required, valid date
  - passengers: required, range 1-9
  - cabinClass: required, one of Economy/Business/FirstClass
- FlightResultDto
  - required display fields from PRD
  - perPassengerPrice and totalPrice required and non-negative
- CreateBookingRequest
  - flightId: required
  - cabinClass: required
  - passengers: required, range 1-9 (should align with selected search)
  - fullName: required, non-empty
  - email: required, valid email format
  - documentType: required, enum (`PassportNumber`, `NationalId`)
  - documentNumber: required
  - server validation rule: server resolves selected flight by `flightId` and validates booking context consistency (including `cabinClass`)
  - route-based document rule: server validates `documentType` and `documentNumber` against selected flight route

Route-dependent document validation:
- International route (origin country != destination country):
  - label: Passport Number
  - accepted documentType: PassportNumber only
  - validator profile: passport rule set
- Domestic route (origin country == destination country):
  - label: National ID
  - accepted documentType: NationalId only
  - validator profile: national-id rule set

### 4.2.1 Provider Raw Data and Normalization Boundary
- GlobalAir and BudgetWings keep raw records in provider-owned JSON files under `Data/providers/`.
- Each provider adapter loads its own JSON schema through `IProviderDataSource`.
- Each adapter maps raw records to `ProviderFlightOffer` (provider-normalized pre-pricing shape).
- Aggregation service applies provider pricing strategy and maps to unified `FlightDetail`.
- API layer projects `FlightDetail` to `FlightResultDto`.
- Search response uses `results` as the collection name.

### 4.2.2 Provider DTO Conversion via Extension Methods
Use provider-specific DTOs plus extension methods to normalize different response shapes into the unified entity.

Design rules:
- Keep each provider DTO in `Features/Flights/Providers/Dtos/`.
- Keep each mapping extension in `Features/Flights/Mappings/`.
- Do not use `dynamic` for provider mapping.
- Parse provider enums/strings explicitly and fail fast on unsupported values.

Example shape:
```csharp
public sealed record GlobalAirFlightDto(
  string FlightId,
  string FlightNumber,
  string Origin,
  string Destination,
  DateTime DepartureTimeUtc,
  DateTime ArrivalTimeUtc,
  string Cabin,
  decimal BaseFare,
  string Currency
);

public static class GlobalAirFlightDtoExtensions
{
  public static ProviderFlightOffer ToProviderFlightOffer(this GlobalAirFlightDto dto)
  {
    return new ProviderFlightOffer(
      FlightId: dto.FlightId,
      Provider: ProviderType.GlobalAir,
      FlightNumber: dto.FlightNumber,
      OriginCode: dto.Origin,
      DestinationCode: dto.Destination,
      DepartureTimeUtc: dto.DepartureTimeUtc,
      ArrivalTimeUtc: dto.ArrivalTimeUtc,
      DurationMinutes: (int)(dto.ArrivalTimeUtc - dto.DepartureTimeUtc).TotalMinutes,
      CabinClass: Enum.Parse<CabinClass>(dto.Cabin, ignoreCase: true),
      BaseFare: dto.BaseFare,
      Currency: dto.Currency
    );
  }
}
```

Equivalent extension methods should exist for BudgetWings (and future providers) so onboarding remains additive and isolated.

For BudgetWings specifically, mapping should explicitly handle its provider-specific field naming differences (for example snake_case payload keys) and convert them via a `ToProviderFlightOffer` extension in `BudgetWingsFlightDtoExtensions`.

### 4.3 Relationships and Constraints
- One search request returns many FlightDetail items.
- One booking references exactly one selected FlightDetail.
- totalPrice must equal perPassengerPrice * passengers, rounded to currency precision.
- Pricing rules are provider-specific and applied before total calculation.
- BudgetWings perPassengerPrice must never be below 29.99.
- `flightId` must reference a valid flight offer that the server can resolve and validate.
- Provider onboarding should not require API contract changes.

### 4.4 Booking Persistence Abstraction (For Discussion)
Use repository abstraction to avoid coupling booking flow to one storage technology.

```csharp
public interface IBookingRepository
{
  Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken);
  Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken);
}
```

Implementation plan:
- Current: `InMemoryBookingRepository` for challenge scope.
- Future: `SqlBookingRepository`, `PostgresBookingRepository`, or `MongoBookingRepository` implementing the same contract.
- Service layer remains unchanged when switching repository implementation.

### 4.5 Common API Response Structure (For Discussion)
Use a consistent response envelope for successful responses to simplify frontend handling and observability.

```csharp
public sealed record ApiResponse<T>(
  T? Data,
  int StatusCode,
  string Message,
  string? Details
)
  where T : class;
```

Guidelines:
- Success responses: return `ApiResponse<T>` with populated `Data`.
- Error responses: prefer RFC Problem Details as primary HTTP error payload (can include `traceId` and timestamp metadata).
- Optional hybrid mode: include a lightweight `ApiResponse<object>` for non-validation business errors if team prefers one envelope everywhere.

## 5. Interface Contracts

### 5.1 Endpoint List
- POST /api/flights/search
- POST /api/bookings

### 5.2 Endpoint Details

#### Endpoint: Search Flights
- Purpose: Search and aggregate available flights from mocked providers with normalized pricing output.
- Method and Route: POST /api/flights/search
- Data behavior: providers read from JSON-backed mock datasets and return deterministic realistic offers for valid searches; empty results are returned only when no flights are available after filtering.
- Request body:
```json
{
  "originCode": "JFK",
  "destinationCode": "LHR",
  "departureDate": "2026-08-15",
  "passengers": 2,
  "cabinClass": "Economy"
}
```
- Query parameters: None
- Success response (200):
```json
{
  "data": {
    "currency": "USD",
    "results": [
      {
        "flightId": "GA-102-20260815",
        "provider": "GlobalAir",
        "flightNumber": "GA102",
        "originCode": "JFK",
        "destinationCode": "LHR",
        "departureTimeUtc": "2026-08-15T08:30:00Z",
        "arrivalTimeUtc": "2026-08-15T16:30:00Z",
        "durationMinutes": 480,
        "cabinClass": "Economy",
        "perPassengerPrice": 184.00,
        "totalPrice": 368.00
      }
    ]
  },
  "statusCode": 200,
  "message": "Flights fetched successfully",
  "details": null
}
```
- Error response:
  - 400 Bad Request (validation failure; RFC Problem Details)
  - 422 Unprocessable Entity (business-rule failure if applicable)
  - 500 Internal Server Error (unexpected processing issue)

#### Endpoint: Create Booking
- Purpose: Confirm booking for a selected flight and passenger details, then return booking reference.
- Method and Route: POST /api/bookings
- Validation behavior: server resolves flight context from `flightId`, validates route-based document rules, and applies booking constraints before persistence.
- Request body:
```json
{
  "flightId": "GA-102-20260815",
  "cabinClass": "Economy",
  "passengers": 2,
  "fullName": "Alex Carter",
  "email": "alex.carter@example.com",
  "documentType": "PassportNumber",
  "documentNumber": "X12345678"
}
```
- Query parameters: None
- Success response (201):
```json
{
  "data": {
    "bookingReference": "SKY-8F2Q1K",
    "status": "Confirmed"
  },
  "statusCode": 201,
  "message": "Booking confirmed",
  "details": null
}
```
- Error response:
  - 400 Bad Request (invalid payload or field format)
  - 409 Conflict (booking already exists/idempotency conflict if implemented)
  - 422 Unprocessable Entity (document type mismatch for route, document number invalid for route, unknown/invalid flightId, or policy validation failure)
  - 500 Internal Server Error

## 6. UI Components (if applicable)

### 6.1 Framework Selection
Angular (from PRD stack choice).

### 6.2 Component Breakdown
- FlightSearchPageComponent
  - Captures search input fields and submission.
  - Triggers loading state and calls search API.
- FlightResultsComponent (table or list)
  - Displays normalized flight fields and pricing distinction.
  - Handles selection action for booking.
- SortControlsComponent
  - Applies in-memory sorting: price asc/desc, duration asc, departure time asc.
- EmptyStateComponent
  - Shows no-results message when search yields no flights.
- BookingPageComponent
  - Shows selected flight summary and full price breakdown.
- PassengerDetailsFormComponent
  - Collects full name, email, and route-based document field.
  - Switches label and validators dynamically.
- BookingConfirmationComponent
  - Displays booking reference after successful confirmation.

### 6.3 State and Interaction Notes
- Search request and results state should remain local to search feature.
- Sorting must transform local results only and must not trigger API requests.
- Selected flight context should be carried to booking page through route state or shared service.
- Document type state should be derived from route country comparison.
- Submit action disabled while booking request is in progress to avoid duplicate requests.

## 7. Acceptance Criteria
- AC-001 Search input validation
  - Given a user opens the flight search page
  - When required fields are missing or passengers are outside 1-9
  - Then the UI prevents submission and displays validation feedback

- AC-002 Aggregated provider results
  - Given a valid search request
  - When the backend aggregates GlobalAir and BudgetWings mocks
  - Then the response contains normalized results with provider, flight details, and pricing fields

- AC-003 Pricing semantics in UI
  - Given search results are displayed
  - When a user reviews any flight option
  - Then the UI shows both total price and per-passenger price as distinct values

- AC-004 Frontend sorting without API re-call
  - Given search results are already loaded
  - When the user changes sort order
  - Then results reorder on the frontend without issuing another search API call

- AC-005 Loading and empty state behavior
  - Given the user submits a search
  - When the request is processing
  - Then a loading indicator is visible until completion

- AC-006 No-match result handling
  - Given a valid search with no matching flights
  - When the response contains an empty result list
  - Then the UI shows a clear empty state message

- AC-007 Booking summary and price breakdown
  - Given a user selects a flight
  - When the booking screen opens
  - Then it displays route, provider, times, cabin class, per-passenger price, passengers, and total price

- AC-008 Route-dependent document behavior
  - Given a selected route is international
  - When passenger details form is shown
  - Then the document label is Passport Number and passport validation rules apply

- AC-009 Domestic document behavior
  - Given a selected route is domestic
  - When passenger details form is shown
  - Then the document label is National ID and national ID validation rules apply

- AC-010 Booking confirmation response
  - Given valid booking details are submitted
  - When the backend creates the booking
  - Then the API returns a booking reference code and the UI shows confirmation

- AC-011 Empty result behavior from mock datasets
  - Given a valid search request where no flights are available after provider/business filtering
  - When the backend aggregates provider responses
  - Then the API returns success with an empty results list

- AC-012 Server-side booking validation
  - Given a booking request includes only necessary booking payload fields
  - When `flightId` cannot be resolved or route-based document type/number validation fails
  - Then the API returns 422 with a business-rule error response

## 8. Error Handling

### 8.1 API Error Response Format
Preferred format: RFC Problem Details (RFC 9457-style usage in ASP.NET) for standardized error payloads.
All unhandled exceptions should be intercepted by the global exception handler and converted to a consistent Problem Details response.

Example:
```json
{
  "type": "https://api.skyroute/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more fields are invalid.",
  "instance": "/api/bookings",
  "errors": {
    "documentNumber": ["Passport Number format is invalid for international route."]
  }
}
```

Optional advanced handling: service/domain layer may use a Results Pattern (Success/Failure typed outcomes) and map failures to Problem Details at the API boundary.

### 8.1.1 Error Response Types
Use these categories consistently across endpoints:

| Error Type | HTTP Status | When It Happens | Response Style |
|---|---|---|---|
| Validation Error | 400 | Missing/invalid request fields, bad enums, date constraints | Problem Details + `errors` dictionary |
| Business Rule Error | 422 | Route/document mismatch, unsupported booking state | Problem Details with business `type` URI |
| Not Found Error | 404 | Booking reference does not exist | Problem Details |
| Conflict Error | 409 | Duplicate booking/idempotency conflict | Problem Details |
| Authentication Error | 401 | Missing/invalid auth token (future capability) | Problem Details |
| Authorization Error | 403 | User lacks permission (future capability) | Problem Details |
| Rate Limit Error | 429 | Too many requests (future capability) | Problem Details + retry guidance |
| Dependency Error | 503 | Provider dependency temporarily unavailable | Problem Details |
| Unexpected Error | 500 | Unhandled server exceptions | Problem Details |

Example business-rule Problem Details payload:
```json
{
  "type": "https://api.skyroute/errors/document-rule",
  "title": "Document validation failed",
  "status": 422,
  "detail": "International route requires Passport Number.",
  "instance": "/api/bookings",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00"
}
```

### 8.2 UI Error Handling Strategy
- Show inline field-level validation for form errors.
- Show non-blocking banner/toast for recoverable API errors.
- Provide retry option for transient search/booking failures.
- Preserve user input in booking form when API call fails.
- Map known status codes to user-friendly messages while keeping technical details out of UI.

## 9. Assumptions

### 9.1 Business Assumptions
- Currency is USD for challenge scope.
- Single-passenger details are sufficient for booking in this version even when passenger count > 1.
- One-way flight search is sufficient for required challenge behavior.

### 9.2 Technical Assumptions
- Backend provider mocks read deterministic, provider-owned JSON datasets.
- Provider adapters normalize raw JSON records into a shared internal model before pricing.
- In-memory data storage is acceptable for booking persistence in local run mode.
- Booking persistence is accessed through repository abstraction to enable DB provider swap later.
- Frontend and backend run as separate local apps with configured base URL/CORS.
- Airport master list is hardcoded in application code or config.

### 9.3 Delivery Assumptions
- Local run is the only required deployment target.
- README will include setup, architecture decisions, trade-offs, and known limitations.
- Interview walkthrough expects explanation of architecture and design rationale.

## 10. Validation and Test Strategy

### 10.1 API Test Coverage Plan
- Unit tests:
  - GlobalAir pricing rule correctness and rounding behavior.
  - BudgetWings pricing rule correctness and minimum fare floor enforcement.
  - Provider DTO extension mappings to unified `ProviderFlightOffer`.
  - Route classification (domestic vs international) and document validation switching.
- Integration tests:
  - POST /api/flights/search returns normalized schema and expected computed prices.
  - POST /api/bookings validates route/document consistency and returns booking reference.
  - Problem Details responses are returned for validation and domain errors.
- Frontend tests (recommended for completeness):
  - Search form validation and submit behavior.
  - Frontend-only sorting behavior without additional HTTP calls.
  - Dynamic document label/validator behavior by route type.

### 10.2 Test Method Naming Convention
Use methodname_condition_result format, for example:
- CalculateGlobalAirFare_BaseFareProvided_ReturnsFareWithSurchargeRounded
- CalculateBudgetWingsFare_DiscountDropsBelowMinimum_ReturnsMinimumFare
- SearchFlights_ValidRequest_ReturnsNormalizedFlightResults
- CreateBooking_InternationalRouteWithNationalId_ReturnsUnprocessableEntity
- CreateBooking_ValidRequest_ReturnsBookingReference

## 11. API Extensibility Roadmap (For Discussion)

### 11.1 Email Sending After Booking Confirmation
- Add an asynchronous notification module that publishes a `BookingConfirmed` domain event.
- Introduce `INotificationService` abstraction with implementations:
  - `EmailNotificationService` (SMTP/provider API)
  - `NoOpNotificationService` for local/dev mode
- Prefer outbox or queue-backed processing for reliability when moving beyond challenge scope.
- Keep booking endpoint fast by avoiding direct email I/O in request pipeline.

### 11.2 User Authentication and Authorization
- Add `Identity` feature boundary with token issuance and user claims.
- Start with JWT bearer authentication and role/policy authorization in API.
- Extend booking contract to associate `UserId` and maintain ownership boundaries.
- Add protected endpoints for "my bookings" while keeping public search endpoint optional by product policy.

### 11.3 Additional Provider Onboarding Pattern
- For each new provider, implement:
  - provider adapter (`IFlightProvider`)
  - provider data source mapping from provider JSON schema
  - pricing strategy (`IProviderPricingStrategy`)
  - provider mapping DTOs and normalization tests
- Add provider health checks and circuit breaker policy once real dependencies are introduced.

### 11.4 Mock Data Strategy and Storage Evolution
- **Mock Provider Input**: Keep provider test data in `Data/providers/*.json` files under source control.
- **Provider Ownership**: Each provider owns its raw schema to simulate real integration differences.
- **Normalization Layer**: Use `IProviderDataSource` + provider adapters to map raw data to unified contracts.
- **Booking Storage Now**: Use `IBookingRepository` with in-memory implementation for local runtime.
- **Booking Storage Later**: Replace repository implementation with SQL/NoSQL provider without changing endpoint payloads.

### 11.5 Cross-Cutting Enhancements
- **Observability**: Add structured logs, metrics, and distributed tracing with `traceId` propagation.
- **Caching**: Add short-lived caching for repeated search queries.
- **Resilience**: Add retries/timeouts for external providers and clear fallback behavior.
- **Versioning**: Introduce API versioning (`/api/v1`) before public rollout.

## 12. Summary
This specification defines a focused but extensible Flight Search and Booking module for SkyRoute using Angular and .NET. It captures provider-specific pricing behavior, frontend sorting and UX requirements, route-dependent document validation, and standardized API contracts/error handling. The architecture is intentionally proposed as discussion-ready while keeping implementation boundaries clear for delivery in local development scope.

## 13. Open Questions
- What exact regex/pattern rules should be enforced for Passport Number and National ID formats?
- Should booking capture one lead traveler only (current assumption) or full traveler details for all passengers in this challenge?
