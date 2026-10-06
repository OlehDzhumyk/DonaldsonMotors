# Donaldson Motors API

This repository contains the **Donaldson Motors** Web API, built with ASP NET Core and PostgreSQL, containerized with Docker and Docker Compose, and secured with ASP NET Core Identity + JWT.

## ☑️ Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* [Docker Engine & Docker Compose](https://docs.docker.com/get-started)


## 🚀 Getting Started

1. **Clone the repo**

   ```bash
   git clone https://github.com/OlehDzhumyk/DonaldsonMotors.git
   cd DonaldsonMotorsAPI
   ```

2. **Environment Variables**

   * Copy `.env.example` to `.env` and fill in values:

     ```dotenv
     ASPNETCORE_ENVIRONMENT=Development
     DB_CONN=Host=db;Database=donaldson;Username=dm_user;Password=dm_pass
     JWT_KEY=your-dev-jwt-secret
     ```

3. **Build & Run with Docker Compose**

   ```bash
   docker-compose up --build -d
   ```

   * API: [http://localhost:5000](http://localhost:5000)
   * PostgreSQL: localhost:5432 (user: dm\_user, pass: dm\_pass)

4. **Apply Migrations**

   ```bash
   docker-compose exec api dotnet ef database update --verbose
   ```

5. **Stop & Clean Up**

   ```bash
   docker-compose down --volumes
   ```

## 📁 Project Structure

```
DonaldsonMotorsAPI.sln
│  .env.example
│  .gitignore
│  docker-compose.yml
│
└─ DonaldsonMotors.API
    │  Dockerfile
    │  .dockerignore
    │  appsettings.json
    │  appsettings.Development.json
    │  Program.cs
    │
    ├─ Controllers
    ├─ Data
    ├─ Entities
    ├─ DTOs
    ├─ Options
    └─ Services
```

## 🔐 Authentication

* **Register**: `POST /api/auth/register`
  Request body: `{ "email": string, "password": string, "fullName": string, "role": string }`

* **Login**:    `POST /api/auth/login`
  Request body: `{ "email": string, "password": string }`

*Response*: `{ "token": "<jwt>" }`
Include `Authorization: Bearer <jwt>` header for protected endpoints.

## 🧪 Running Tests

```bash
cd DonaldsonMotors.Tests
dotnet test
```

## 📦 Deployment

1. Build and push Docker image to your registry.
2. On your production server, set real environment variables (no `.env`):

   ```bash
   export ConnectionStrings__Default="Host=prod-db;Database=donaldson;Username=prod;Password=ProdPass"
   export Jwt__Key="your-production-jwt-key"
   ```
3. Run:

   ```bash
   docker-compose up -d
   ```

## ⚙️ Configuration Hierarchy

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Docker Compose `.env` (Development)
4. OS Environment Variables (Production)
5. Command-line Arguments
