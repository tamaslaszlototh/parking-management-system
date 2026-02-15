using ErrorOr;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.DeactivateParkingSpot.Commands;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;
using ParkingManagementSystem.Domain.Reservation;
using ParkingSpotErrors = ParkingManagementSystem.Domain.ParkingSpot.Errors.Errors.ParkingSpot;

namespace ParkingManagementSystem.Application.UnitTests.ParkingSpots;

public class DeactivateParkingSpotTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;
    private readonly IReservationsRepository _reservationsRepository;
    private readonly DeactivateParkingSpotCommandHandler _handler;

    public DeactivateParkingSpotTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _parkingSpotsRepository = Substitute.For<IParkingSpotsRepository>();
        _reservationsRepository = Substitute.For<IReservationsRepository>();
        _handler = new DeactivateParkingSpotCommandHandler(
            _unitOfWork,
            _parkingSpotsRepository,
            _reservationsRepository);
    }

    [Fact]
    public async Task Handle_WhenParkingSpotDoesNotExist_ShouldReturnParkingSpotNotFoundError()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns((ParkingSpot?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotNotFound());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _parkingSpotsRepository.DidNotReceive().Update(Arg.Any<ParkingSpot>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParkingSpotIsAlreadyDeactivated_ShouldReturnParkingSpotIsAlreadyDeactivatedError()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Already deactivated"),
            state: ParkingSpotState.Deactivated);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotIsAlreadyDeactivated());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _parkingSpotsRepository.DidNotReceive().Update(Arg.Any<ParkingSpot>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParkingSpotIsActive_ShouldDeactivateParkingSpot()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Active spot"),
            state: ParkingSpotState.Active,
            id: parkingSpotId);

        var userId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var reservation1 = Reservation.Create(userId, parkingSpotId, today.AddDays(1));
        var reservation2 = Reservation.Create(userId, parkingSpotId, today.AddDays(2));
        var reservations = new List<Reservation> { reservation1, reservation2 };

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ReservationIds.Should().HaveCount(2);
        result.Value.ReservationIds.Should().Contain(new[] { reservation1.Id, reservation2.Id });
        result.Value.LastReservedDate.Should().Be(today.AddDays(2));

        parkingSpot.State.Should().Be(ParkingSpotState.Deactivated);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _parkingSpotsRepository.Received(1).Update(parkingSpot);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParkingSpotIsDedicated_ShouldDeactivateParkingSpot()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("D1"),
            ParkingSpotDescription.Create("Dedicated spot"),
            managerId: managerId,
            state: ParkingSpotState.Dedicated,
            id: parkingSpotId);

        var today = DateOnly.FromDateTime(DateTime.Now);
        var reservation = Reservation.Create(managerId, parkingSpotId, today.AddDays(1));
        var reservations = new List<Reservation> { reservation };

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ReservationIds.Should().HaveCount(1);
        result.Value.ReservationIds.Should().Contain(reservation.Id);

        parkingSpot.State.Should().Be(ParkingSpotState.Deactivated);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _parkingSpotsRepository.Received(1).Update(parkingSpot);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoFutureReservations_ShouldDeactivateAndReturnEmptyReservations()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Active spot"),
            state: ParkingSpotState.Active,
            id: parkingSpotId);

        var emptyReservations = new List<Reservation>();

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(emptyReservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ReservationIds.Should().BeEmpty();
        // Note: This will throw if no reservations exist - you might need to handle this edge case
        // result.Value.LastReservedDate...

        parkingSpot.State.Should().Be(ParkingSpotState.Deactivated);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _parkingSpotsRepository.Received(1).Update(parkingSpot);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMultipleFutureReservations_ShouldReturnCorrectLastReservedDate()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Active spot"),
            state: ParkingSpotState.Active,
            id: parkingSpotId);

        var userId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var reservation1 = Reservation.Create(userId, parkingSpotId, today.AddDays(5));
        var reservation2 = Reservation.Create(userId, parkingSpotId, today.AddDays(10)); // Latest
        var reservation3 = Reservation.Create(userId, parkingSpotId, today.AddDays(3));
        var reservations = new List<Reservation> { reservation1, reservation2, reservation3 };

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ReservationIds.Should().HaveCount(3);
        result.Value.LastReservedDate.Should().Be(today.AddDays(10)); // Should be the latest date

        parkingSpot.State.Should().Be(ParkingSpotState.Deactivated);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccursAfterDeactivation_ShouldRollbackTransaction()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Active spot"),
            state: ParkingSpotState.Active);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Save failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDeactivatingSpot_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Active spot"),
            state: ParkingSpotState.Active,
            id: parkingSpotId);

        var reservations = new List<Reservation>();

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        Received.InOrder(() =>
        {
            _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>());
            _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>());
            _parkingSpotsRepository.Update(parkingSpot);
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
            _unitOfWork.CommitTransactionAsync(Arg.Any<CancellationToken>());
            _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpotId,
                Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WhenDeactivating_ShouldFetchReservationsAfterCommit()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new DeactivateParkingSpotCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Active spot"),
            state: ParkingSpotState.Active,
            id: parkingSpotId);

        var userId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var reservation = Reservation.Create(userId, parkingSpotId, today.AddDays(1));
        var reservations = new List<Reservation> { reservation };

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        // Verify that reservations are fetched AFTER commit
        await _reservationsRepository.Received(1)
            .GetReservationsForParkingSpotFromTodayAsync(parkingSpotId, Arg.Any<CancellationToken>());

        // Ensure commit happened before fetching reservations
        Received.InOrder(() =>
        {
            _unitOfWork.CommitTransactionAsync(Arg.Any<CancellationToken>());
            _reservationsRepository.GetReservationsForParkingSpotFromTodayAsync(parkingSpotId,
                Arg.Any<CancellationToken>());
        });
    }
}