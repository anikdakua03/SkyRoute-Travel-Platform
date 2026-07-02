# SkyRoute Platform

SkyRoute Platform is a flight search and booking demo built with an Angular UI and a .NET backend. The app lets a user search mocked provider data, compare normalized results, book a selected flight, and look up a booking by reference.

## Table of contents

1. [Project Architechture](#architecture)
    - [Backend](#backend)
    - [Frontend](#frontend)
2. [Project Structure](#project-structure)
3. [Prerequisites](#prerequisites)
4. [Technology stack](#technology-stack)
    - [Backend](#backend-1)
    - [Frontend](#frontend-1)
5. [Installation and setup](#installation--setup)
    - [Clone the Repository](#1-clone-the-repository)
    - [Verify Prerequisites (Optional but Recommended)](#2-verify-prerequisites-optional-but-recommended)
    - [Backend Setup (API)](#3-backend-setup-api)
    - [Frontend Setup (UI)](#4-frontend-setup-ui)
    - [Build Commands](#5-build-commands)
    - [Run Locally (Two Terminals)](#6-run-locally-two-terminals)
6. [API summary](#api-behavior-summary)
7. [Trade offs and limitations](#trade-offs-and-limitations)
8. [Future scopes](#future-scope)
9. [Notes](#notes-for-review)

## What Changed

- Flight search and booking UI was implemented in Angular with dark-mode-first styling.
- Backend endpoints are consumed through focused flight and booking API services.
- Booking lookup by reference was added as a separate UI flow.
- The UI handles API response envelopes and error parsing in the component layer so service code stays minimal.

## Architecture

### Backend

The API follows a vertical slice style structure. Feature code is grouped by capability, such as Flights and Bookings, while reusable concerns live under Shared. That keeps request handling, services, contracts, and provider logic close together and makes future feature additions easier to isolate.

This structure is intended to stay extensible. New capabilities such as users, authentication, email notifications, or additional provider integrations can be added by introducing new feature folders without reshaping the entire codebase.

Error handling is intentionally standardized:

- Successful and failure responses use a simplified custom API response envelope so the UI can explicitly handle both cases.
- Unhandled exceptions are captured by GlobalExceptionHandler and converted to RFC Problem Details.
- The current response shape is intentionally lightweight and can be expanded with richer error metadata later.

### Frontend

The UI is organized by route and feature:

- Flight search and result sorting are handled on the search page.
- Booking confirmation is handled on a dedicated booking page.
- Booking reference lookup is handled on a separate page.

The UI keeps state local to each feature, uses reactive forms, and applies dark mode by default with a custom visual style.

## Project Structure

```text
SkyRoutePlatform/
├── SkyRoutePlatform.API/                    # Backend API (.NET 10)
│   ├── Program.cs                           # App bootstrap and DI registrations
│   ├── Features/
│   │   ├── Flights/
│   │   │   ├── Contracts/                   # Search request/response contracts
│   │   │   ├── Endpoints/                   # Minimal API endpoint mappings
│   │   │   ├── Mappings/                    # DTO/entity mapping extensions
│   │   │   ├── Pricing/                     # Provider-specific pricing strategies
│   │   │   ├── Providers/                   # GlobalAir/BudgetWings adapters + DTOs
│   │   │   └── Services/                    # Search orchestration logic
│   │   └── Bookings/
│   │       ├── Contracts/                   # Create/Get booking contracts
│   │       ├── Endpoints/                   # Booking endpoint mappings
│   │       ├── Repositories/                # In-memory booking persistence
│   │       └── Services/                    # Booking validation and workflow
│   ├── Shared/
│   │   ├── Models/                          # Common domain models and enums
│   │   ├── Responses/                       # Custom ApiResponse envelope
│   │   └── Validation/                      # Document validation strategies
│   ├── Middlewares/
│   │   └── GlobalExceptionHandlerMiddleware.cs
│   └── Data/providers/                      # Mock provider JSON datasets
│
├── SkyRoutePlatform.UI/                     # Frontend UI (Angular 22)
│   ├── src/app/
│   │   ├── app.ts                           # Root app component
│   │   ├── app.routes.ts                    # Lazy-loaded route definitions
│   │   ├── core/
│   │   │   ├── data/                        # Airport metadata
│   │   │   ├── models/                      # Shared UI models/contracts
│   │   │   └── services/                    # Flights API + Bookings API services
│   │   └── features/
│   │       ├── flight-search/               # Search form, results, sorting
│   │       ├── booking/                     # Booking confirmation flow
│   │       └── booking-reference/           # Lookup by booking reference
│   └── src/styles.scss                      # Global dark-mode-first theme styles
│
├── SkyRoutePlatform.Tests/                  # Backend unit tests
└── spec.md                                  # Detailed implementation specification
```

## Prerequisites

- .NET 10 SDK
- Node.js with npm
- Angular dependencies installed in the UI project

## Technology Stack

### Backend
- **.NET 10** - Modern cloud-ready framework
- **C# 14** - Latest language features
- **ASP.NET Core** - REST API with minimal APIs
- **In-Memory Data Store** - For development/testing

### Frontend
- **Angular 22.0.2** - Standalone components, signals, reactive forms
- **TypeScript** - Strict mode enabled
- **SCSS** - CSS variables with automatic dark mode
- **Node.js / npm** - Dependency management

## Installation & Setup

### 1. Clone the Repository

```powershell
git clone <repository-url>
cd SkyRoutePlatform
```

### 2. Verify Prerequisites (Optional but Recommended)

```powershell
node --version
npm --version
dotnet --version
```

### 3. Backend Setup (API)

```powershell
cd .\SkyRoutePlatform.API
dotnet restore
dotnet build
dotnet run
```

API base URL in this workspace: http://localhost:5118

Optional backend tests:

```powershell
cd ..\SkyRoutePlatform.Tests
dotnet test
```

### 4. Frontend Setup (UI)

```powershell
cd ..\SkyRoutePlatform.UI
npm install
npm run build
npm start
```

UI dev server default URL: http://localhost:4200

The UI currently targets API base URL http://localhost:5118. If you change API ports, update UI API service base URLs accordingly.

### 5. Build Commands

Backend:

```powershell
cd .\SkyRoutePlatform.API
dotnet build
```

Frontend:

```powershell
cd ..\SkyRoutePlatform.UI
npm run build
```

### 6. Run Locally (Two Terminals)

Terminal 1 (API):

```powershell
cd <project-root>\SkyRoutePlatform.API
dotnet run
```

Terminal 2 (UI):

```powershell
cd <project-root>\SkyRoutePlatform.UI
npm start
```

### 7. Quick Validation Flow

1. Search flights (example: JFK to LHR, Economy, 1 passenger).
2. Change sorting in the results list and confirm no extra search call is required.
3. Select a flight and create a booking.
4. Copy booking reference from confirmation.
5. Open Booking Lookup and fetch booking details with that reference.

Development note:

- OpenAPI/Scalar tooling is available in Development for API exploration.

## API Behavior Summary

- Flight search: POST /api/flights/search
- Booking creation: POST /api/bookings
- Booking lookup: GET /api/bookings/{bookingReference}

The API returns provider-normalized flight results and booking details in a consistent response envelope. Validation failures and business-rule failures are surfaced clearly, while unexpected exceptions are mapped to Problem Details.

## Trade-offs and Limitations

- The custom API response envelope is intentionally simple and could be improved with a more structured error model, richer metadata, and stronger consistency across all failure types.
	- Today, the response shape works well for the current demo flows, but it does not yet separate domain errors, validation errors, and transport errors into strongly typed categories.
	- The envelope is adequate for the current UI, but a larger app would benefit from a more explicit contract for error codes and field-level metadata.
- Error parsing is currently handled in the UI components, which keeps services minimal but duplicates a small amount of logic that could later be centralized in a thin shared helper.
	- This keeps the service layer thin and easier to test.
	- It also means multiple pages repeat similar message extraction logic until a shared UI helper is introduced.
- Seat inventory is not modeled. There is no logic for total seats, remaining seats, seat selection, or seat map rendering.
	- That keeps the implementation focused on search and booking flow validation rather than inventory management.
- Coupon, promo code, payment, tax, refund, and cancellation flows are not implemented.
	- Those flows would require additional business rules, API contracts, and UX states.
- Provider data is mocked locally rather than sourced from live airline integrations.
	- This makes the demo deterministic and runnable without external dependencies.
	- It also means the app does not cover integration failure modes from third-party APIs.
- Booking persistence is in-memory, so booked data does not survive application restarts.
	- This is appropriate for a local interview/demo workflow but not for production use.
- Booking and flight flows are intentionally simplified for demonstration purposes rather than full production checkout complexity.
- There is no user account system, saved traveler profile, or booking history.
- Structured logging, distributed tracing, and production observability integrations are not yet present.
	- Operational debugging is therefore more limited than it would be in a production-grade deployment.

## Future Scope

- Adding authentication and authorization
	- Protect booking and lookup operations behind authenticated user sessions.
	- Add role- or policy-based access control for admin and traveler scenarios.
- Realtime external providers
	- Replace mocked provider JSON with live adapter implementations.
	- Keep the provider abstraction so new suppliers can be onboarded without changing the UI contract.
- User data and persistence in a database
	- Store bookings, traveler details, and search history in a durable store.
	- Introduce repository and migration strategies for persistence.
- Realtime notifications
	- Notify users when bookings are created or updated.
	- Use push, email, or SMS depending on the future product direction.
- Structured logs
	- Add correlation IDs and richer request/response telemetry.
	- Use logs to improve supportability and observability.
- Aspire integration for a detailed dashboard covering both API and UI
	- Use Aspire to visualize services, dependencies, and runtime health in one place.
	- Extend the dashboard to make local development and demo walkthroughs easier.

## Notes for Review

- The app uses a dark-mode-first visual theme.
- Search and sorting happen on the client side after the initial API call.
- Booking document rules switch between Passport Number and National ID based on the route.
- The booking reference page can be used to verify bookings after confirmation.
