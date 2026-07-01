---
description: 'Comprehensive Copilot instructions for HotelStay project - consolidates Angular, C#, and ASP.NET best practices'
applyTo: '**/*.ts, **/*.cs, **/*.json, **/*.html, **/*.scss'
---

# HotelStay Project - Copilot Instructions

This document serves as the master reference for GitHub Copilot when generating code across the HotelStay project. It consolidates three core instruction sets to ensure consistent, high-quality code generation.

## Quick Reference by Technology

- **Frontend (Angular/TypeScript)**: See [Angular Instructions](#angular-instructions)
- **Backend (.NET/C#)**: See [C# Instructions](#csharp-instructions) + [ASP.NET REST API Instructions](#aspnetcore-rest-api-instructions)
- **Full Stack**: All sections apply

---

## Angular Instructions

**Applies to**: TypeScript files in `hotel-stay-ui/src/app/**`

### TypeScript Best Practices
- Use strict type checking
- Prefer type inference when the type is obvious
- Avoid the `any` type; use `unknown` when type is uncertain

### Angular Best Practices
- Always use standalone components over NgModules
- Must NOT set `standalone: true` inside Angular decorators. It's the default in Angular v20+.
- Do NOT set `changeDetection: ChangeDetectionStrategy.OnPush` explicitly. `OnPush` is the default in Angular v22+.
- Use signals for state management
- Implement lazy loading for feature routes
- Do NOT use the `@HostBinding` and `@HostListener` decorators. Put host bindings inside the `host` object of the `@Component` or `@Directive` decorator instead
- Use `NgOptimizedImage` for all static images (does not work for inline base64 images)

### Components
- Keep components small and focused on a single responsibility
- Use `input()` and `output()` functions instead of decorators
- Use `computed()` for derived state
- Prefer inline templates for small components
- Prefer Signal Forms (`@angular/forms/signals`) for new forms. They are stable in Angular v22+ and provide signal-based state, type-safe field access, and schema-based validation
- When not using Signal Forms, prefer Reactive forms instead of Template-driven ones
- Do NOT use `ngClass`, use `class` bindings instead
- Do NOT use `ngStyle`, use `style` bindings instead
- When using external templates/styles, use paths relative to the component TS file

### State Management
- Use signals for local component state
- Use `computed()` for derived state
- Keep state transformations pure and predictable
- Do NOT use `mutate` on signals, use `update` or `set` instead

### Templates
- Keep templates simple and avoid complex logic
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- Use the async pipe to handle observables
- Do not assume globals like (`new Date()`) are available
- Use `===` for equality checks instead of `==` and for false use `=== false` to be clear and explicit

### Services
- Design services around a single responsibility
- Use the `providedIn: 'root'` option for singleton services
- Prefer the `@Service` decorator over `@Injectable({providedIn: 'root'})` for new singleton services (Angular v22+)
- Use the `inject()` function instead of constructor injection

### Accessibility Requirements
- It MUST pass all AXE checks
- It MUST follow all WCAG AA minimums, including focus management, color contrast, and ARIA attributes

**Full Details**: See `.github/instructions/angular.instructions.md`

---

## C# Instructions

**Applies to**: All C# files in `HotelStay.API/**` and `HotelStay.Tests/**`

### C# Version & Features
- Always use the latest version C# (currently C# 14 features)
- Write clear and concise comments for each function

### Naming Conventions
- **PascalCase**: Class names, method names, public members
- **camelCase**: Private fields and local variables
- **Interface Prefix "I"**: e.g., `IUserService`

### Code Style and Conventions
- **Namespaces**: Always use file-scoped namespaces to reduce indentation
- **Identifier Casing**: 
  - PascalCase for Class names, Methods, Properties, and Public Fields
  - camelCase for local variables and method parameters
  - _camelCase for private, read-only fields
- **Braces**: Use Allman style (opening brace on a new line)
- **Explicit Typing**: Prefer strong explicit types over 'var' unless the type is explicitly obvious on the right side of the assignment

### Modern C# Features
- Mark the class as `sealed` by default (if required, can be changed according to needs)
- **Collection Expressions**: Use bracket syntax `[]` instead of `new List<T>()` or `new T[]`
- **Pattern Matching**: Lean into switch expressions and pattern matching over nested if-else structures
- **Record Types**: Use native record types for immutable DTOs and value objects

### Formatting Rules
- Apply code-formatting style defined in `.editorconfig`
- Prefer file-scoped namespace declarations
- Insert a newline before the opening curly brace of any code block (after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.)
- Ensure final return statement of a method is on its own line
- Use `nameof` instead of string literals when referring to member names
- Ensure XML doc comments are created for any public APIs (include `<example>` and `<code>` documentation when applicable)

### Nullable Reference Types
- Declare variables non-nullable, check for `null` at entry points
- Always use `is null` or `is not null` instead of `== null` or `!= null`
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null

### Exception Handling & Edge Cases
- Handle edge cases with clear exception handling
- Create consistent error responses across the API
- Explain problem details (RFC 9457) implementation for standardized error responses

### Testing
- Always include test cases for critical paths of the application
- Create unit tests for controllers, services, and data access layers
- Do not emit "Act", "Arrange" or "Assert" comments in test code
- Copy existing style in nearby files for test method names and capitalization
- Demonstrate integration testing approaches for API endpoints
- Show how to mock dependencies for effective testing

**Full Details**: See `.github/instructions/csharp.instructions.md`

---

## ASP.NET Core REST API Instructions

**Applies to**: API structure, Controllers, and Minimal APIs in `HotelStay.API/**`

### API Design Fundamentals
- Explain REST architectural principles and how they apply to ASP.NET Core APIs
- Design meaningful resource-oriented URLs with appropriate HTTP verb usage
- Understand the difference between traditional controller-based APIs and Minimal APIs
- Handle proper status codes, content negotiation, and response formatting
- Choose Controllers vs. Minimal APIs based on project requirements

### Project Setup & Structure
- Explain the purpose of each generated file and folder
- Organize code using feature folders or domain-driven design principles
- Show proper separation of concerns with models, services, and data access layers
- Explain `Program.cs` and configuration system in ASP.NET Core 10 including environment-specific settings
- Adhere to SOLID principles in API design
- Also use design pattern whereever appropriate.

### Building Controller-Based APIs
- Create RESTful controllers with proper resource naming and HTTP verb implementation
- Use attribute routing and its advantages over conventional routing
- Implement model binding, validation, and the `[ApiController]` attribute
- Apply dependency injection within controllers
- Choose appropriate action return types (`IActionResult`, `ActionResult<T>`, specific return types)

### Implementing Minimal APIs
- Implement endpoints using the Minimal API syntax
- Explain endpoint routing system and route group organization
- Demonstrate parameter binding, validation, and dependency injection in Minimal APIs
- Structure larger Minimal API applications to maintain readability

### Data Access Patterns
- Implement a data access layer using Entity Framework Core
- Choose appropriate data store options (SQL Server, SQLite, In-Memory)
- Implement repository pattern when beneficial
- Show how to implement database migrations and data seeding
- Explain efficient query patterns to avoid common performance issues

### Authentication & Authorization
- Implement authentication using JWT Bearer tokens
- Explain OAuth 2.0 and OpenID Connect concepts
- Implement role-based and policy-based authorization
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD)
- Secure both controller-based and Minimal APIs consistently

### Validation & Error Handling
- Implement model validation using data annotations and FluentValidation
- Explain the validation pipeline and customize validation responses
- Demonstrate global exception handling strategy using middleware
- Create consistent error responses across the API
- Implement problem details (RFC 9457) for standardized error responses

### API Versioning & Documentation
- Implement API versioning strategies
- Demonstrate Swagger/OpenAPI implementation with proper documentation
- Document endpoints, parameters, responses, and authentication
- Explain versioning in both controller-based and Minimal APIs

### Logging & Monitoring
- Implement structured logging using Serilog or other providers
- Explain logging levels and when to use each
- Demonstrate integration with Application Insights for telemetry collection
- Show custom telemetry and correlation IDs for request tracking
- Monitor API performance, errors, and usage patterns

### Performance Optimization
- Implement caching strategies (in-memory, distributed, response caching)
- Explain asynchronous programming patterns and their importance for API performance
- Demonstrate pagination, filtering, and sorting for large data sets
- Show compression and other performance optimizations
- Measure and benchmark API performance

### Deployment & DevOps
- Containerize API using .NET's built-in container support (`dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`)
- Explain differences between manual Dockerfile creation and .NET's container publishing features
- Implement CI/CD pipelines for ASP.NET Core applications
- Demonstrate deployment to Azure App Service, Azure Container Apps, or other hosting options
- Implement health checks and readiness probes
- Configure environment-specific settings for different deployment stages

**Full Details**: See `.github/instructions/aspnet-rest-apis.instructions.md`

---

## Cross-Cutting Concerns

### Code Quality Standards
- Make only high confidence suggestions when reviewing code changes
- Write code with good maintainability practices, including comments on why certain design decisions were made
- For libraries or external dependencies, mention their usage and purpose in comments

### File Organization
- Maintain clear separation between frontend (`hotel-stay-ui/src/app/`) and backend (`HotelStay.API/`)
- Follow domain-driven design when organizing code
- Keep related functionality together in coherent modules/features

### Documentation
- Include XML doc comments for all public APIs
- Document configuration and environment-specific behavior
- Maintain clear README files explaining project structure and setup

---

## How to Use These Instructions

1. **For Angular/TypeScript Development**: Copilot will reference the [Angular Instructions](#angular-instructions) section
2. **For C# Development**: Copilot will reference the [C# Instructions](#csharp-instructions) section
3. **For ASP.NET Core API Development**: Copilot will reference both [C# Instructions](#csharp-instructions) and [ASP.NET Core REST API Instructions](#aspnetcore-rest-api-instructions)
4. **For Full Stack Development**: All sections apply

When Copilot generates code, it will follow these consolidated guidelines to ensure consistent, high-quality, and maintainable code across your entire SkyRoute project.

---

**Last Updated**: 2026-06-24
**Project**: SkyRoute v2.2
**Scope**: Frontend (Angular 22+) + Backend (.NET 10 / C# 14)
