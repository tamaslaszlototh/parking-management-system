using ErrorOr;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.RemoveDedicatedParkingSpotAssignment;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;
using ParkingSpotErrors = ParkingManagementSystem.Domain.ParkingSpot.Errors.Errors.ParkingSpot;

namespace ParkingManagementSystem.Application.UnitTests.ParkingSpots;

public class RemoveDedicatedParkingSpotAssignmentTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;
    private readonly RemoveDedicatedParkingSpotAssignmentCommandHandler _handler;

    public RemoveDedicatedParkingSpotAssignmentTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _parkingSpotsRepository = Substitute.For<IParkingSpotsRepository>();
        _handler = new RemoveDedicatedParkingSpotAssignmentCommandHandler(
            _unitOfWork,
            _parkingSpotsRepository);
    }

    [Fact]
    public async Task Handle_WhenParkingSpotDoesNotExist_ShouldReturnParkingSpotNotFoundError()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns((ParkingSpot?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotNotFound());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParkingSpotIsNotDedicated_ShouldReturnParkingSpotIsNotDedicatedError()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Active spot"),
            state: ParkingSpotState.Active,
            id: parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotIsNotDedicated());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParkingSpotIsDedicatedButHasNoManager_ShouldReturnParkingSpotIsNotDedicatedError()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("D1"),
            ParkingSpotDescription.Create("Dedicated spot without manager"),
            managerId: null,
            state: ParkingSpotState.Dedicated,
            id: parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotIsNotDedicated());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParkingSpotIsDedicatedWithManager_ShouldRemoveManagerAssignment()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("D1"),
            ParkingSpotDescription.Create("Manager's dedicated spot"),
            managerId: managerId,
            state: ParkingSpotState.Dedicated,
            id: parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        parkingSpot.ManagerId.Should().BeNull();

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRemovingAssignment_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("D1"),
            ParkingSpotDescription.Create("Manager's dedicated spot"),
            managerId: managerId,
            state: ParkingSpotState.Dedicated,
            id: parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        Received.InOrder(() =>
        {
            _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>());
            _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
            _unitOfWork.CommitTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

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
    public async Task Handle_WhenExceptionOccursDuringSave_ShouldRollbackTransaction()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("D1"),
            ParkingSpotDescription.Create("Manager's dedicated spot"),
            managerId: managerId,
            state: ParkingSpotState.Dedicated,
            id: parkingSpotId);

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
    public async Task Handle_WhenParkingSpotIsDeactivatedWithManager_ShouldReturnParkingSpotIsNotDedicatedError()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("D1"),
            ParkingSpotDescription.Create("Deactivated spot"),
            managerId: managerId,
            state: ParkingSpotState.Deactivated,
            id: parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotIsNotDedicated());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSuccessfullyRemovingAssignment_ShouldNotRollback()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("D1"),
            ParkingSpotDescription.Create("Manager's dedicated spot"),
            managerId: managerId,
            state: ParkingSpotState.Dedicated,
            id: parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AfterRemovingAssignment_ManagerIdShouldBeNull()
    {
        // Arrange
        var parkingSpotId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("D1"),
            ParkingSpotDescription.Create("Manager's dedicated spot"),
            managerId: managerId,
            state: ParkingSpotState.Dedicated,
            id: parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Verify manager is assigned before
        parkingSpot.ManagerId.Should().Be(managerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        parkingSpot.ManagerId.Should().BeNull(); // Manager assignment removed
    }
}