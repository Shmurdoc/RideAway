# RideAway

Ride-hailing API built on .NET 6 following Clean Architecture with CQRS via MediatR.

## Features

- User and driver accounts with role-based authorization (JWT bearer tokens)
- Ride lifecycle: request, accept, collect rider, complete, cancel
- Driver location updates and proximity-based ride matching within a 10 km radius
- Distance and fare calculation backed by the Google Maps API
- Payment processing via Stripe (test mode)
- Notifications via SendGrid (email) and Twilio (SMS)
- Global exception handling, structured logging to file (Serilog), Swagger UI in development

## Solution Structure

```
RideAway.Domain/          Entities, value objects, enums, domain exceptions
RideAway.Application/     CQRS commands/queries/handlers, DTOs, interfaces (use cases)
RideAway.Infrastructure/  EF Core persistence, Google Maps/Stripe/Twilio/SendGrid clients,
                          JWT generation, DI registration
RideAway.API/             ASP.NET Core controllers, exception middleware, composition root
RideAway.Tests/           Unit tests (xUnit + Moq + FluentAssertions)
```

Layer rules: `Domain` has no dependencies, `Application` depends only on `Domain`,
`Infrastructure` implements `Application` interfaces, and `API` is the composition root
that wires everything through `Program.cs`. See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Tech Stack

- .NET 6, ASP.NET Core, EF Core 6, SQL Server
- MediatR (CQRS), AutoMapper
- JWT bearer authentication (Microsoft.IdentityModel 7.1.2)
- Google Maps Geocoding / Distance Matrix APIs, Stripe, SendGrid, Twilio
- xUnit, Moq, FluentAssertions

## Prerequisites

- .NET SDK 6.0
- SQL Server (local or remote)
- API keys: Google Maps, Stripe, SendGrid, Twilio (test keys are sufficient)

## Configuration

All settings live in `RideAway.API/appsettings.json` unless noted:

| Setting | Purpose | Required |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Yes |
| `GoogleMaps:ApiKey` | Geocoding + distance matrix calls | Yes (matching/geocoding) |
| `Stripe:SecretKey` | Payment processing (test key ok) | Yes (payments) |
| `Jwt:Issuer` | Token issuer claim | Yes |
| `Jwt:Key` | Token signing key; set in `appsettings.Development.json` for local dev | Yes (auth) |
| `SendGrid:ApiKey` | Email notifications | Optional |
| `Twilio:AccountSid`, `Twilio:AuthToken` | SMS notifications | Optional |

## Getting Started

1. Clone the repository.
2. Add your connection string and API keys (see Configuration).
3. Apply the EF Core migrations:

   ```bash
   dotnet ef database update --project RideAway.API
   ```

4. Run the API:

   ```bash
   dotnet run --project RideAway.API
   ```

5. Browse to https://localhost:7039/swagger in development for the interactive UI.

## Authentication

All endpoints except `POST /api/auth/login` and `POST /api/User` require a bearer token.

1. Create a user: `POST /api/User` with `{ "createUserDTO": { "name": "...", "email": "...", "role": "Rider" } }`.
2. Login: `POST /api/auth/login` with the same credentials to receive a JWT.
3. Send the token as `Authorization: Bearer <token>`.

Driver-only endpoints (`/api/drivers/*`) require a user with role `Driver`.

## API Reference

### Auth

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/auth/login` | None | Authenticate and return a JWT |

### Users

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/User` | None | Create a user (rider or driver) |
| GET | `/api/User/{id}` | Bearer | Get a user by id |
| GET | `/api/User/available-rides?startLocation=&endLocation=&ride=` | Bearer | Find nearby drivers within 10 km |
| POST | `/api/User/request` | Bearer | Request a ride |
| POST | `/api/User/cancel` | Bearer | Cancel a ride |
| POST | `/api/User/process` | Bearer | Process a payment |

### Drivers

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/drivers/update-location` | Driver | Update current location |
| POST | `/api/drivers/collect-rider` | Driver | Start a ride after picking up the rider |
| POST | `/api/drivers/process-payment` | Driver | Process payment for a ride |
| POST | `/api/drivers/accept` | Driver | Accept a ride request |
| POST | `/api/drivers/cancel` | Driver | Cancel a ride |

## Build & Test

```bash
dotnet build RideAway.sln --nologo
dotnet test RideAway.Tests/RideAway.Tests.csproj --nologo
```

The solution builds with zero warnings (nullable reference types enabled) and the test
suite covers the application use cases, services, and API controllers.

## Database

EF Core migrations live in `RideAway.API/Migrations`. The model includes users, vehicles,
rides, and payments. Ride matching uses a geocoded driver location and the Google Maps
distance matrix to rank candidates within the configured radius.
