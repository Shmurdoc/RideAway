# Architecture Guide

This document explains how the RideAway codebase is organized and how a request flows
through it. It is written for developers who are new to the project.

## Layer Overview

```
RideAway.API             (composition root, HTTP)
      |
      v
RideAway.Application     (use cases: commands, queries, handlers, interfaces)
      |
      v
RideAway.Infrastructure  (persistence, external APIs, DI wiring)
      |
      v
RideAway.Domain          (entities, value objects, exceptions)
```

Dependencies point **inward** only:

- `RideAway.Domain` — no project references at all. Contains entities (`User`, `Ride`,
  `Payment`, `Vehicle`), value objects (`GeoLocation`), enums (`RideStatus`,
  `UserRole`), and domain exceptions (`RideNotFoundException` and friends).
- `RideAway.Application` — references `Domain` only. Contains MediatR commands,
  queries, and their handlers under `Features/`, plus interfaces that the
  infrastructure layer implements: `IRepositories/`, `IServices/`.
- `RideAway.Infrastructure` — references `Application` and `Domain`. Implements EF
  Core persistence, Google Maps / Stripe / SendGrid / Twilio clients, JWT token
  generation, and the service registrations in `DependencyInjection/`.
- `RideAway.API` — references everything and acts as the composition root. Controllers
  only translate HTTP into MediatR messages; they contain no business logic.

## Why CQRS + MediatR

Every operation is expressed as a small message:

- **Command** — something that changes state (`RequestRideCommand`, `AcceptRideCommand`).
- **Query** — something that reads state (`GetAvailableRidesQuery`, `GetUserByIdQuery`).

A **handler** receives the message and returns a result. Controllers never talk to the
database or services directly; they `Send` a message through `IMediator` and the
matching handler does the work. This keeps the API layer thin and makes each use case
individually testable.

## Request Lifecycle (Example: Request a Ride)

1. `UserController.RequestRide` receives a `CreateRideRequestDTO` from the HTTP body.
2. It maps the DTO into a `RequestRideCommand` and calls `IMediator.Send(command)`.
3. The `LoggingBehavior` pipeline behavior logs the start and completion of the
   handler (registered in `AddInfrastructure`).
4. `RequestRideCommandHandler` runs: it validates the ride category, geocodes the
   pickup and destination addresses via `IGeoCodingService`, calculates an estimated
   fare via `IFareCalculationService`, and persists the ride through the `IUnitOfWork`.
5. The controller returns the resulting `Ride` as `200 OK`.

## Where to Find Things

| Concern | Location |
| --- | --- |
| Controllers | `RideAway.API/Controllers/` |
| Global error handling | `RideAway.API/Middleware/ExceptionMiddleware.cs` |
| Commands / queries / handlers | `RideAway.Application/Features/<Feature>/` |
| DTOs | `RideAway.Application/DTOs/` |
| Repository interfaces | `RideAway.Application/IRepositories/` |
| Service interfaces | `RideAway.Application/IServices/` |
| EF Core DbContext + repositories | `RideAway.Infrastructure/Persistence/` |
| External clients (Maps, Stripe, etc.) | `RideAway.Infrastructure/` subfolders |
| DI registration | `RideAway.Infrastructure/DependencyInjection/` |
| Entities and value objects | `RideAway.Domain/` |

## Adding a New Feature

1. Add the entity to `RideAway.Domain/Entities` if a new aggregate is needed.
2. Add the command/query record and its handler under
   `RideAway.Application/Features/<Feature>/`.
3. Add repository or service interfaces in `RideAway.Application` and implement them
   in `RideAway.Infrastructure`.
4. Register the new types in
   `RideAway.Infrastructure/DependencyInjection/DependencyInjection.cs`.
5. Expose the operation from a controller in `RideAway.API/Controllers`.
6. Write unit tests in `RideAway.Tests` mirroring the handler structure, then run:

   ```bash
   dotnet test RideAway.Tests/RideAway.Tests.csproj --nologo
   ```

## Key Conventions

- **Nullable reference types are enabled and the build must stay warning-free.**
- Validation happens in handlers (throwing domain exceptions) rather than in
  controllers; `ExceptionMiddleware` converts domain exceptions into appropriate
  HTTP status codes.
- External API calls are wrapped in interfaces (`IGoogleMapsApi`, `IGeoCodingService`,
  `IPaymentProcessingService`) so they can be mocked in tests.
- Secrets live in configuration files, never in source code. The development JWT
  signing key in `appsettings.Development.json` must be replaced for any
  non-development environment.
