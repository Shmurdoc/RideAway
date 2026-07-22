# RideAway

Ride-hailing API platform built on .NET 6 with Clean Architecture.

## Project Structure

```
RideAway.Domain/          -- Entities, value objects, enums, exceptions, domain services
RideAway.Application/     -- Use cases (CQRS commands/queries/handlers), DTOs, repository interfaces, service interfaces
RideAway.Infrastructure/  -- Persistence (EF Core), external services (Google Maps, Stripe), authentication, notifications
RideAway.API/             -- ASP.NET Core controllers, middleware, DI composition root
RideAway.Tests/           -- Unit tests (xUnit + Moq + FluentAssertions)
IntegrationTest/          -- Integration tests (xUnit)
ArchitectureTest/         -- ArchUnit tests for layer boundary enforcement
```

## Tech Stack

- .NET 6, ASP.NET Core, EF Core 6, SQL Server
- MediatR (CQRS), AutoMapper, FluentValidation
- xUnit, Moq, FluentAssertions, Bogus, NetArchTest
- Stripe, Google Maps API, JWT

## Getting Started

1. Clone the repo
2. Set connection string in `appsettings.Development.json`
3. Run EF migrations: `dotnet ef database update --project RideAway.API`
4. Run: `dotnet run --project RideAway.API`

## Build & Test

```bash
dotnet build RideAway.sln
dotnet test RideAway.sln
```

## Architecture

Clean Architecture (onion) with CQRS via MediatR:

- **API layer** owns composition (DI registration lives in Infrastructure, called from API Program.cs)
- **Application layer** defines interfaces, handles commands/queries, contains zero DI registration
- **Infrastructure layer** implements persistence, external service clients, and DI wiring
- **Domain layer** has no external dependencies
