# Donaldson Motors API

The ASP.NET Core Web API behind [Donaldson Motors](../README.md). It runs on .NET 10 with EF Core, PostgreSQL 17 and
ASP.NET Core Identity (JWT). It has 43 endpoints, and Swagger UI is at http://localhost:5080/swagger when the API
runs in Development.

## Running it

The quickest way is `docker compose up --build` from the repository root (see the [main README](../README.md#running-it)).
To run the API from source you need the [.NET 10 SDK](https://dotnet.microsoft.com/download):

```bash
docker compose up db mailpit                                    # from the repository root
dotnet run --project backend/DonaldsonMotors/DonaldsonMotors.API
```

On start-up the API applies any pending migrations and, when `SeedDemoData` is true, seeds the demo accounts,
cars, parts and bookings. It listens on http://localhost:5080. Port 5000 is avoided because macOS uses it for AirPlay.

### Configuration

Settings come from `appsettings.json`, then `appsettings.Development.json`, then environment variables.
Use `__` for nesting, for example `EmailSettings__SmtpHost`. Docker Compose passes them from
[`.env.example`](../.env.example).

| Setting                        | What it does                                                                |
|--------------------------------|-----------------------------------------------------------------------------|
| `ConnectionStrings:Default`    | PostgreSQL connection string                                                |
| `Jwt:Key`, `Issuer`, `Audience`| Token signing. Use a long random key anywhere but your own machine          |
| `Jwt:ExpiryMinutes`            | Token lifetime (60 by default)                                              |
| `SeedDemoData`                 | Seed demo accounts and bookings on start-up                                 |
| `EmailSettings:*`              | SMTP server and sender. An empty `SmtpHost` turns email off                 |

### Migrations

```bash
dotnet tool install --global dotnet-ef        # once
cd backend/DonaldsonMotors/DonaldsonMotors.API
dotnet ef migrations add <Name> -o Data/Migrations
```

There is no need to run `dotnet ef database update`: the API migrates the database itself on start-up.
An integration test fails if the model has changes that are not in a migration.

## How it's built

```
DonaldsonMotors.API/
  Controllers/      thin: read the request, call a service, return the result
  Services/         business rules (BookingService, ScheduleService, ...), JWT and email
    Email/          queue and background sender for emails
  Repositories/     EF Core repositories behind a unit of work
  Data/             AppDbContext, entities, migrations, DbInitializer (seed data)
  DTOs/ Mappers/    request and response shapes, and mapping to and from entities
  Exceptions/       domain exceptions and ApiExceptionHandler
  Options/          typed settings (JWT, email) and GarageTime (UK local time)
  Templates/        Razor templates for emails and the invoice
DonaldsonMotors.Tests/
  Unit/             services tested with Moq and an in-memory fake unit of work
  Integration/      the real API against PostgreSQL in Testcontainers
Bruno_Donaldson_Motors_API_Reqests/   55 example requests for Bruno
```

A few things worth knowing:

- **Errors.** Services throw domain exceptions such as `BookingNotFoundException` or `SlotUnavailableException`.
  [`ApiExceptionHandler`](DonaldsonMotors/DonaldsonMotors.API/Exceptions/ApiExceptionHandler.cs) maps each one to a
  status code and returns a [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) body. Missing records are
  404, duplicates and records still in use are 409, broken business rules are 400, and anything unexpected is a
  logged 500. Controllers have no try/catch.
- **Roles.** There are five: Customer, Manager, Mechanic, StockController and AccountsClerk. Each action declares its
  own `[Authorize(Roles = ...)]`, because when attributes are stacked on a controller and an action, all of them
  must pass.
- **Booking rules** live in `BookingService`. A booking moves Pending → Assigned → InProgress → AwaitingPayment →
  Paid, or to Cancelled. Each method checks the current status and who is calling.
- **Concurrent requests.** The database has the final say when two requests race:
  - A partial unique index on `Bookings.SlotStart`, ignoring cancelled bookings, stops a slot being booked twice.
  - `Part` uses PostgreSQL's `xmin` as a concurrency token, so two stock changes can't overwrite each other.
  - The request that loses gets 409.
- **Prices.** Each part used on a job stores its unit price at that moment (`JobPart.UnitPrice`), so changing a
  part's price later doesn't change invoices that were already issued.
- **Time.** Slots are generated in Europe/London time and stored in UTC (`GarageTime`), so a 09:00 slot stays 09:00
  through the BST change.
- **Email.** `EmailService` renders a Razor template with RazorLight and puts the message on an in-memory queue.
  `EmailBackgroundService` sends it with MailKit, retrying up to three times, so a slow mail server doesn't hold up
  the request. Emails still in the queue are lost if the app stops. A database outbox would fix that.

## Tests

```bash
cd backend/DonaldsonMotors
dotnet test                                                     # needs Docker for the integration tests
dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage"
```

- **Unit tests (37)** cover:
  - booking rules: slots in the past or already taken, someone else's car, a mechanic double-booked, who may
    start and finish a job, stock running out, prices fixed when the job is finished, payment and cancellation;
  - slot generation: lunch, holidays, booked slots and the BST change;
  - the email queue: retries, giving up, and a full queue.
- **Integration tests (43)** start the API with `WebApplicationFactory` against a PostgreSQL 17 container, with the
  same migrations and seed data as Docker Compose. They cover:
  - registration and login, and what each role may and may not do;
  - one booking going from Pending to Paid across four roles;
  - profiles, vehicles, suppliers, parts, service types and holidays;
  - the database constraints for concurrent bookings and stock changes;
  - that the migrations match the model.

Line coverage is about 85%, not counting the generated migrations. CI builds in Release with warnings treated as
errors.
