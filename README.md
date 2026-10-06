# Donaldson Motors

[![CI](https://github.com/OlehDzhumyk/DonaldsonMotors/actions/workflows/ci.yml/badge.svg)](https://github.com/OlehDzhumyk/DonaldsonMotors/actions/workflows/ci.yml)

A booking and workshop system for a car garage, built as my final project for the HND in Software Development.
Customers book a service online; the manager assigns a mechanic; the mechanic records the work and the parts used;
the accounts clerk takes payment. Customers get an email at each step, ending with an itemised invoice.

**Stack:** ASP.NET Core Web API (.NET 10), EF Core + PostgreSQL, ASP.NET Core Identity with JWT,
React 19 + TypeScript + Redux Toolkit, Docker Compose, xUnit + Moq + Testcontainers, Vitest.

<table>
  <tr>
    <td><img src="docs/screenshots/booking.png" alt="Booking a service in three steps" width="400"></td>
    <td><img src="docs/screenshots/dashboard.png" alt="Manager's board of bookings by status" width="400"></td>
  </tr>
  <tr>
    <td align="center">Customer: booking a service</td>
    <td align="center">Manager: bookings by status</td>
  </tr>
  <tr>
    <td><img src="docs/screenshots/assign-mechanic.png" alt="Assigning a free mechanic" width="400"></td>
    <td><img src="docs/screenshots/finish-job.png" alt="Mechanic finishing a job with parts" width="400"></td>
  </tr>
  <tr>
    <td align="center">Manager: assigning a mechanic who is free</td>
    <td align="center">Mechanic: finishing a job and sending the invoice</td>
  </tr>
  <tr>
    <td><img src="docs/screenshots/stock.png" alt="Stock and suppliers" width="400"></td>
    <td><img src="docs/screenshots/my-bookings.png" alt="Customer's bookings" width="400"></td>
  </tr>
  <tr>
    <td align="center">Stock controller: parts and suppliers</td>
    <td align="center">Customer: upcoming bookings</td>
  </tr>
</table>

## Features

- **Five roles** with JWT authentication: Customer, Manager, Mechanic, Stock Controller and Accounts Clerk.
  Each role gets its own pages, and the API checks the role on every endpoint.
- **Booking.** Customers add their cars, pick a service and choose a free slot. Slots come from the garage's
  working hours, lunch break and holidays, which the manager can change. A slot that is already booked is not offered.
  Times are UK local time, so they stay at 09:00 through BST and GMT.
- **Booking life cycle:** Pending → Assigned → In progress → Awaiting payment → Paid, or Cancelled.
  The service enforces each step: only the assigned mechanic can start or finish a job, and a manager can only assign
  a mechanic who has no overlapping job.
- **Jobs and stock.** When mechanics finish a job, they record the work, the labour cost and the parts used.
  The parts come out of stock, and stock can't go below zero.
- **Emails** rendered from Razor templates with RazorLight and sent with MailKit: welcome, booking confirmed,
  mechanic assigned, job done (with the amount due), paid invoice and cancellation. In Docker they go to
  [Mailpit](https://mailpit.axllent.org/), so you can read them at http://localhost:8025.
- **Layered API:** controllers → services → repositories with a unit of work, DTOs and mappers. There are
  43 endpoints and a [Bruno](https://www.usebruno.com/) collection of 55 example requests in
  [`backend/DonaldsonMotors/Bruno_Donaldson_Motors_API_Reqests`](backend/DonaldsonMotors/Bruno_Donaldson_Motors_API_Reqests).
  Swagger UI is at http://localhost:5080/swagger.

## Running it

You need Docker. This starts the React app, the API, PostgreSQL and Mailpit. The API applies the migrations and
seeds demo data on first start:

```bash
git clone https://github.com/OlehDzhumyk/DonaldsonMotors.git
cd DonaldsonMotors
docker compose up --build
```

| What              | Where                          |
|-------------------|--------------------------------|
| Web app           | http://localhost:3000          |
| API and Swagger   | http://localhost:5080/swagger  |
| Emails (Mailpit)  | http://localhost:8025          |

No `.env` file is needed. To change a setting, such as using a real SMTP server, copy [`.env.example`](.env.example) to `.env`.

### Demo accounts

All demo accounts use the password `Password123!`. The login page also lists them, and clicking one fills in the form.

| Role             | Email                    |
|------------------|--------------------------|
| Customer         | customer@donaldson.com   |
| Manager          | manager@donaldson.com    |
| Mechanic         | mechanic@donaldson.com   |
| Stock controller | stock@donaldson.com      |
| Accounts clerk   | accounts@donaldson.com   |

The demo data has a booking in every state, including finished jobs with parts, invoices and payments.

### Running from source

- **API:** needs the [.NET 10 SDK](https://dotnet.microsoft.com/download). Start the database and Mailpit with
  `docker compose up db mailpit`, then run `dotnet run --project backend/DonaldsonMotors/DonaldsonMotors.API`.
  It reads `appsettings.Development.json`, seeds the same demo data and listens on port 5080.
- **Frontend:** needs Node 22. Run `cd frontend && npm ci && npm run dev`, then open http://localhost:5173.
  The Vite dev server forwards `/api` to port 5080.

## Tests

```bash
cd backend/DonaldsonMotors && dotnet test     # needs Docker for the integration tests
cd frontend && npm test
```

- **Unit tests (32, xUnit + Moq)** cover the booking rules:
  - slots in the past or already taken;
  - booking someone else's car;
  - a mechanic assigned to two jobs at once;
  - who may start and finish a job;
  - stock running out, payment, cancellation;
  - search results limited to the caller's own bookings;
  - slot generation: lunch, holidays, booked slots and the BST change.
- **Integration tests (39)** start the real API with `WebApplicationFactory` against PostgreSQL in a
  [Testcontainers](https://dotnet.testcontainers.org/) container, with the same migrations and seed data as
  Docker Compose. They test:
  - registration and login;
  - what each role may and may not do;
  - one booking going all the way from Pending to Paid across four roles;
  - profiles and vehicles, suppliers, parts, service types and holidays;
  - that the migrations match the model.
- **Frontend tests (15, Vitest + Testing Library)** cover route protection by role, form validation, the
  booking card and error messages.

API line coverage is about 75%, not counting the generated migrations. GitHub Actions runs the backend build
(with warnings treated as errors), both test suites with a coverage summary, the frontend type check, lint and build,
and the Docker image builds.

The TypeScript is `strict` with `noUncheckedIndexedAccess`, and ESLint uses `typescript-eslint`'s
`strictTypeChecked` rules with no errors.

## Project structure

```
backend/DonaldsonMotors/
  DonaldsonMotors.API/
    Controllers/       Auth, Bookings, Schedule, Users, Parts, Suppliers, ServiceTypes
    Services/          business rules (BookingService, ScheduleService, ...), JWT and email
    Repositories/      EF Core repositories behind a unit of work
    Data/              DbContext, entities, migrations, seed data
    DTOs/ Mappers/     request/response shapes and mapping
    Templates/         Razor email and invoice templates
  DonaldsonMotors.Tests/   Unit/ (Moq) and Integration/ (Testcontainers)
frontend/src/
  pages/               one page per route
  components/          BookingCard, Modal, forms, layout
  api/                 typed API clients (axios)
  app/                 Redux store and auth slice
docs/                  screenshots and the database diagram (pgAdmin ERD)
```

## What I'd do next

- Online payment (for example Stripe Checkout) in place of the clerk marking the job as paid.
- Size slots by the service. Today every slot is two hours, even for a 30-minute tyre change, and the garage takes
  one booking per slot whatever the number of mechanics.
- Refresh tokens. At the moment the user is logged out when the 60-minute JWT expires.
- An email for each invoice in PDF form, and a page where customers can download their past invoices.

## Licence

[MIT](LICENSE)
