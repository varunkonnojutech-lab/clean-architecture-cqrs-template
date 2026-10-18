# Clean Architecture + CQRS Template

[![Build](https://github.com/varunkonnojutech-lab/clean-architecture-cqrs-template/actions/workflows/ci.yml/badge.svg)](https://github.com/varunkonnojutech-lab/clean-architecture-cqrs-template/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/varunkonnojutech-lab/clean-architecture-cqrs-template)

A starter template for **order management** built with **Clean Architecture**, **CQRS**, and **MediatR** on .NET 8. Clone it when you want a backend that stays maintainable as it grows: a rich domain model with no dependencies, an application layer organized around commands and queries, EF Core persistence, and a thin Web API.

## Who is this for

Backend developers moving past simple CRUD who want to see how professional .NET systems separate domain logic, application use-cases, and infrastructure — and how to keep that separation honest.

## What you'll learn

- Clean Architecture layering: Domain → Application → Infrastructure → WebApi
- CQRS with MediatR: commands, queries, and handlers
- The `Result<T>` pattern instead of exceptions for expected failures
- FluentValidation in a MediatR pipeline behavior (validation without plumbing in handlers)
- Rich domain modeling: aggregates, value objects (`Money` as a `readonly record struct`), guarded state transitions
- Repository + Unit of Work over EF Core, with hand-written test fakes (no mocking libraries)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — or click **Open in GitHub Codespaces** above for a ready-made environment in your browser, no install needed.
- [Docker](https://www.docker.com/) (optional, for the container run)

## Quick start

```bash
git clone https://github.com/varunkonnojutech-lab/clean-architecture-cqrs-template.git
cd clean-architecture-cqrs-template
dotnet build
dotnet test
cd src/WebApi
dotnet run
```

Open `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`) and try it:

```bash
# Create an order
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Acme Corp","items":[{"productName":"Widget","quantity":2,"unitPrice":9.99}]}'

# Fetch it (use the id from the response)
curl http://localhost:5000/api/orders/<id>
```

The SQLite database file (`orders.db`) is created automatically next to the running app.

### Run with Docker

```bash
docker build -f src/WebApi/Dockerfile -t clean-arch-cqrs .
docker run -p 8080:8080 clean-arch-cqrs
```

## How it works

Dependencies flow inward only. The Domain layer knows nothing about anything else; Application depends on Domain; Infrastructure implements Application's interfaces; WebApi composes everything.

```
 ┌──────────┐
 │  WebApi  │   Controllers, middleware, DI composition root
 └────┬─────┘
      │ depends on
 ┌────▼─────────┐
 │ Application  │   CQRS via MediatR, FluentValidation, DTOs, repository interfaces
 └────┬─────────┘
      │ depends on
 ┌────▼────┐          ┌────────────────┐
 │ Domain  │ ◄────────│ Infrastructure │   EF Core (SQLite), repository implementations
 └─────────┘          └────────────────┘
      ▲ implements interfaces defined in Application
```

**Request flow for `POST /api/orders`:**

```
Controller → IMediator.Send(CreateOrderCommand)
             → ValidationBehavior (FluentValidation; throws ValidationException on failure)
             → CreateOrderCommandHandler (builds Order aggregate, persists via IOrderRepository + IUnitOfWork)
             → Result<Guid> back to the controller
```

**Design decisions worth knowing:**

- **Validation:** the MediatR pipeline behavior throws FluentValidation's `ValidationException` when a request fails validation. A global exception-handling middleware in `Program.cs` maps it to `400 Bad Request` with a JSON error body. This keeps handlers free of validation plumbing and avoids reflection tricks to construct `Result<T>` failures.
- **Result pattern:** handlers return `Result<T>` (`IsSuccess` / `Error` / `Value`) instead of throwing for expected failures (e.g. order not found). Controllers translate these into `400` / `404` responses.
- **Money as a complex type:** `Money` is a `readonly record struct` value object. EF Core maps it with `ComplexProperty` (EF Core 8 supports structs as complex types; classic `OwnsOne` owned types require reference types), stored as `UnitPriceAmount` / `UnitPriceCurrency` columns on the order-items table.
- **Order aggregate:** `Order` guards its own invariants — status transitions (`Pending → Confirmed → Shipped → Delivered`, cancel only from `Pending`/`Confirmed`), items can only be added while pending, quantities must be positive.

## Project structure

```
CleanArchitectureCqrsTemplate.sln
src/
  Domain/                          # No dependencies. Entities, value objects, enums.
    Entities/Order.cs              # Order aggregate root (status machine, invariants)
    Entities/OrderItem.cs
    ValueObjects/Money.cs          # readonly record struct: Amount + 3-letter Currency
    Enums/OrderStatus.cs
  Application/                     # Depends on Domain. MediatR + FluentValidation.
    Common/Result.cs               # Result<T> success/failure wrapper
    Dtos/OrderDto.cs               # Read models + FromOrder mapping
    Interfaces/                    # IOrderRepository, IUnitOfWork (implemented by Infrastructure)
    Features/Orders/
      Commands/CreateOrder/        # CreateOrderCommand + validator + handler
      Queries/GetOrderById/        # GetOrderByIdQuery + handler
    Behaviors/ValidationBehavior.cs# MediatR pipeline: FluentValidation → throws on failure
  Infrastructure/                  # Depends on Application. EF Core + SQLite.
    Persistence/AppDbContext.cs
    Persistence/Repositories/OrderRepository.cs
    Persistence/UnitOfWork.cs
    DependencyInjection.cs         # AddInfrastructure(IServiceCollection, IConfiguration)
  WebApi/                          # Depends on Application + Infrastructure.
    Controllers/OrdersController.cs# POST /api/orders, GET /api/orders/{id}
    Program.cs                     # Serilog, MediatR, Swagger, global exception middleware
    appsettings.json               # ConnectionStrings:DefaultConnection = Data Source=orders.db
    Dockerfile
tests/
  Application.Tests/               # xUnit. Hand-written fakes (no mocking libraries).
    CreateOrderCommandHandlerTests.cs
.devcontainer/                     # One-click GitHub Codespaces setup
.github/workflows/ci.yml           # restore → build → test on push/PR to main
```

## Next steps

Ready to split this into independently deployable services? Continue with [dotnet-microservices](https://github.com/varunkonnojutech-lab/dotnet-microservices) — Catalog + Ordering services, a YARP gateway, and RabbitMQ events.

## License

MIT — see [LICENSE](LICENSE).
