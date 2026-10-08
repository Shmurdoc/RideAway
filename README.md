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

All settings live in `RideAway.API/appsettings.json` unless noted. Secrets are never
checked into the repository:

| Setting | Purpose | Required |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Yes |
| `GoogleMaps:ApiKey` | Geocoding + distance matrix calls | Yes (matching/geocoding) |
| `Stripe:SecretKey` | Payment processing (test key ok) | Yes (payments) |
| `Jwt:Issuer` | Token issuer claim | Yes |
| `Jwt:Audience` | Token audience claim (audience is validated) | Yes |
| `Jwt:Key` | Token signing key; set in `appsettings.Development.json` for local dev | Yes (auth) |
| `Stripe:WebhookSecret` | Signing secret for the Stripe webhook | Yes (card payments) |
| `RateLimiting:Strict` / `RateLimiting:Global` | Request budgets per IP | No (defaults on) |
| `SendGrid:ApiKey` | Email notifications | Optional |
| `Twilio:AccountSid`, `Twilio:AuthToken` | SMS notifications | Optional |
| `Cors:AllowedOrigins` | Comma-separated allowed browser origins | Yes (Production) |

The API **fails fast at startup** when required settings are missing, so a misconfigured
deployment cannot silently serve traffic without auth or a database.

### Production

For production, supply secrets via environment variables (or your secret manager). The
app reads them from the environment, so no secrets need to be committed:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=RideAway;..."
export Jwt__Key="<32+ char signing key>"
export Jwt__Issuer="RideAway"
export Jwt__Audience="RideAway"
export GoogleMaps__ApiKey="..."
export Stripe__SecretKey="..."
export Stripe__WebhookSecret="whsec_..."
export Cors__AllowedOrigins__0="https://app.rideaway.com"
```

`ASPNETCORE_ENVIRONMENT=Production` must be set so `appsettings.Production.json` is loaded.

### Health checks

`GET /health` reports the API and database health (200 `Healthy` / 503 `Unhealthy`).
Wire it into your orchestrator's health probe (e.g., Docker `HEALTHCHECK` or Kubernetes
liveness/readiness).

### Docker

A multi-stage `Dockerfile` builds the app and runs it as an unprivileged user on `:8080`:

```bash
docker build -t rideaway-api .
docker run -p 8080:8080 --env-file production.env rideaway-api
curl http://localhost:8080/health
```

The CI pipeline publishes the app and uploads it as a `rideaway-api` artifact.

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

1. Create a user: `POST /api/User` with `{ "createUserDTO": { "name": "...", "email": "...", "password": "<12+ chars>", "role": "Rider" } }`.
   The `role` field is only honoured for `Driver`; every other value (including `Admin`)
   is registered as a `Rider`, so a self-registering caller cannot escalate.
2. Login: `POST /api/auth/login` with the same credentials to receive a JWT.
3. Send the token as `Authorization: Bearer <token>`.

Driver-only endpoints (`/api/drivers/*`) require a user with role `Driver`.

The identity of the caller always comes from the token. Client-supplied user or driver
ids are rejected, so one user cannot act on another user's ride, payment or location.

Both credential endpoints are rate limited per IP (10 requests / 5 minutes by default).

## API Reference

### Auth

| Method | Endpoint | Auth | Rate limited | Description |
| --- | --- | --- | --- | --- |
| POST | `/api/auth/login` | None | Yes | Authenticate and return a JWT |

### Users

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/User` | None | Create a user (rate limited; always a Rider unless `Driver` is requested) |
| GET | `/api/User/{id}` | Bearer | Get a profile - your own, or anyone's if you are an admin |
| GET | `/api/User/available-rides?startLocation=&endLocation=&ride=` | Bearer | Find nearby drivers within 10 km |
| POST | `/api/User/request` | Bearer | Request a ride (you become its rider) |
| POST | `/api/User/cancel` | Rider/Driver | Cancel a ride you are party to |
| POST | `/api/User/process` | Rider | Start payment for a completed ride you own |

### Drivers

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/drivers/update-location` | Driver | Update your own location |
| POST | `/api/drivers/accept` | Driver | Accept a requested ride |
| POST | `/api/drivers/collect-rider` | Driver | Start a ride you are assigned to |
| POST | `/api/drivers/complete` | Driver | Complete a ride you are assigned to |
| POST | `/api/drivers/confirm-cash` | Driver | Confirm cash collected, settling the ride |
| POST | `/api/drivers/cancel` | Driver | Cancel a ride you are party to |

### Webhooks

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/webhooks/stripe` | Stripe signature | Settles a card payment (`checkout.session.completed`) |

## Payments

The amount charged is **always** the ride's server-computed fare - it is never taken
from the request. A ride moves through
`Requested -> Accepted -> InProgress -> Completed -> Paid`, and each transition is
validated by the `Ride` aggregate, so a cancelled or in-flight ride cannot be settled
and a settled ride cannot be settled twice.

Card payments are **pending** until Stripe confirms them. Creating a checkout session
is not treated as payment: the ride is marked paid only when a signature-verified
`checkout.session.completed` webhook arrives and the settled amount and currency match
what was owed. Cash payments are also pending until the assigned driver confirms
collection via `/api/drivers/confirm-cash`.

To accept card payments, register `POST /api/webhooks/stripe` as a webhook endpoint in
the Stripe dashboard for the `checkout.session.completed` event, and set the signing
secret as `Stripe:WebhookSecret`.

## Build & Test

```bash
dotnet build RideAway.sln --nologo
dotnet test RideAway.Tests/RideAway.Tests.csproj --nologo
```

The solution builds with zero warnings (nullable reference types enabled) and the test
suite covers the application use cases, services, and API controllers.

The repository pins the .NET SDK in `global.json` so local builds match CI exactly.

## Known Limitations

**Action required:** a Google Maps API key (`AIzaSyDud...`) and an internal hostname were
committed to this repository's history in 2025. The key must be **rotated in Google Cloud
Console** and the history rewritten (`git filter-repo`) - deleting the line does not undo
publication. Until that is done, treat the key as public.

Remaining gaps:

- The project targets .NET 6, which is out of support (no security patches). Upgrading to
  a supported LTS (e.g., .NET 8) is recommended before production.
- AutoMapper 11 is affected by a high-severity DoS advisory
  ([GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x)). The patched
  versions (15.1.1) require .NET 8, so the upgrade is deferred to the framework migration.
  The API only maps server-side entities to DTOs; no attacker-controlled deep object
  graphs are mapped.
- PBKDF2 iteration count is 100,000. That is below current OWASP guidance for
  PBKDF2-HMAC-SHA256 (600,000+). The stored hash format carries no version or cost
  parameter, so raising it needs a rehash-on-login migration.
- Command validation is hand-rolled inside handlers rather than using a validation
  library, so new commands must be reviewed for input validation by hand.
- Cancellation does not void an in-flight Stripe Checkout Session; a customer can still
  complete payment for a cancelled ride (the ride will not settle, and the payment is
  recorded, but the money is not automatically refunded).
- No optimistic concurrency token (`rowversion`) on `Ride`/`Payment`. The unique index on
  `Payments(RideId)` prevents double settlement at the database level, but concurrent
  ride-state transitions rely on the aggregate's status checks alone.
- Email and phone numbers are stored in cleartext. Enable SQL Server TDE and consider
  Always Encrypted for `PhoneNumber`.
- `TrustServerCertificate=True` is set on the sample connection string, which disables
  database TLS validation. Remove it and install the CA in production.
- Tests are unit-level only; add integration tests (e.g., WebApplicationFactory) for
  end-to-end coverage of the API layer, including the authorization matrix.
- Rate limiting is per-process and in-memory, so it does not apply across multiple
  instances. Use a gateway or Redis-backed limiter when scaling horizontally.

## Database

EF Core migrations live in `RideAway.API/Migrations`. The model includes users, vehicles,
rides, and payments. Ride matching uses a geocoded driver location and the Google Maps
distance matrix to rank candidates within the configured radius.
