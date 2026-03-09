# AGENTS.md - Parking Management System

This document provides guidelines for agents working in this codebase.

## Project Overview

- **Architecture**: Clean Architecture with CQRS pattern using MediatR
- **Framework**: .NET 10.0 with ASP.NET Core
- **Testing**: xUnit, FluentAssertions, NSubstitute
- **Database**: Entity Framework Core (code-first migrations)
- **Error Handling**: ErrorOr library for functional error handling

## Build & Test Commands

### Build Solution
```bash
dotnet build
```

### Run All Tests
```bash
dotnet test
```

### Run Single Test
```bash
# By test method name
dotnet test --filter "FullyQualifiedName~ReserveParkingSpotTests.Handle_WhenUserDoesNotExist_ShouldReturnUserNotFoundError"

# By test class
dotnet test --filter "FullyQualifiedName~ReserveParkingSpotTests"

# Shorter form (matches any test containing the string)
dotnet test ---filter "Name~Handle_WhenUserDoesNotExist"
```

### Run Tests in Specific Project
```bash
dotnet test ParkingManagementSystem.Application.UnitTests
```

### Build Release
```bash
dotnet build -c Release
```

## Code Style Guidelines

### Imports

- Use explicit namespace imports (no implicit usings in domain layer)
- Group imports: System → Third-party → Project-specific
- Use alias for error classes to avoid conflicts:
  ```csharp
  using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;
  using ReservationErrors = ParkingManagementSystem.Domain.Reservation.Errors.Errors.Reservation;
  ```

### Formatting

- Use file-scoped namespaces
- Place constructor at top of class, after fields
- Order members: fields → constructor → public methods → private methods
- Use expression-bodied members where appropriate
- Use collection expressions `[]` instead of `new List<T>()`

### Types

- Use nullable reference types (`string?`, `Guid?`)
- Use `DateOnly` for dates, `DateTime` for timestamps
- Use `Guid` for identifiers
- Use `ErrorOr<T>` for command handler return types (return `ErrorOr<Success>` or `ErrorOr<TResponse>`)
- Use domain value objects (e.g., `ParkingSpotName`, `Email`) instead of primitives

### Naming Conventions

- **Classes/Interfaces**: PascalCase (`ReserveParkingSpotCommandHandler`)
- **Methods**: PascalCase (`Handle`, `GetFreeParkingSpots`)
- **Private fields**: `_camelCase` (`_reservationsRepository`)
- **Parameters**: camelCase (`cancellationToken`)
- **Commands/Queries**: `[Operation]Command` / `[Operation]Query`
- **Command Handlers**: `[Operation]CommandHandler`
- **Test Methods**: `MethodName_WhenCondition_ShouldOutcome`

### Error Handling

- Return `ErrorOr<T>` from command handlers
- Use domain error classes in `Errors.{Entity}.cs` files
- Create errors using factory methods:
  ```csharp
  return UserErrors.UserNotFound();
  return Errors.Reservation.UserAlreadyHasReservationForDates(dates);
  ```
- Wrap operations in try-catch with transaction rollback:
  ```csharp
  try {
      // operations
      await _unitOfWork.CommitTransactionAsync(cancellationToken);
  } catch {
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      return Error.Failure();
  }
  ```

### Domain-Driven Design

- Use aggregate roots for entities with invariants
- Use value objects for primitives with validation
- Use domain events for cross-aggregate communication
- Follow strict layers: Domain → Application → Infrastructure → API

### Validation

- Use FluentValidation for command validation
- Register validators in `DependencyInjection.cs`
- Use validation behavior in MediatR pipeline

### Testing

- Test file location: `{Feature}.Tests` namespace
- Use `[Fact]` for test methods
- Follow AAA pattern: Arrange → Act → Assert
- Use `FluentAssertions` for assertions:
  ```csharp
  result.IsError.Should().BeTrue();
  result.FirstError.Should().Be(UserErrors.UserNotFound());
  ```
- Use `NSubstitute` for mocking:
  ```csharp
  _reservationsRepository.Received(1).AddAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
  ```

### Project Structure

```
ParkingManagementSystem/
├── ParkingManagementSystem.Api/           # HTTP layer
├── ParkingManagementSystem.Application/   # CQRS commands/queries
├── ParkingManagementSystem.Contracts/     # Request/Response DTOs
├── ParkingManagementSystem.Domain/        # Domain entities, value objects
├── ParkingManagementSystem.Infrastructure/# EF Core, auth, persistence
└── ParkingManagementSystem.Application.UnitTests/
```

### Database

- Use code-first migrations
- Add migration: `dotnet ef migrations add MigrationName`
- Apply migrations: `dotnet ef database update`
- Configure in `appsettings.json`

### Configuration

- Use `UserSecretsId` for development secrets
- JWT settings in `appsettings.json`
- Environment-specific configs: `appsettings.Development.json`
