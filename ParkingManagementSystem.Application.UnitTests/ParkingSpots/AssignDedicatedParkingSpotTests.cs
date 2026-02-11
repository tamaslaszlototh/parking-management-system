using ErrorOr;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ParkingManagementSystem.Application.AssignDedicatedParkingSpot;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;
using ParkingManagementSystem.Domain.User;
using ParkingManagementSystem.Domain.User.Enums;
using ParkingManagementSystem.Domain.User.ValueObjects;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;
using ParkingSpotErrors = ParkingManagementSystem.Domain.ParkingSpot.Errors.Errors.ParkingSpot;

namespace ParkingManagementSystem.Application.UnitTests.ParkingSpots;

public class AssignDedicatedParkingSpotTests
{
    private readonly IUserRepository _userRepository;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AssignDedicatedParkingSpotCommandHandler _handler;

    public AssignDedicatedParkingSpotTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _parkingSpotsRepository = Substitute.For<IParkingSpotsRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new AssignDedicatedParkingSpotCommandHandler(
            _userRepository,
            _parkingSpotsRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenParkingSpotDoesNotExist_ShouldReturnParkingSpotIsNotDedicatedError()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns((ParkingSpot?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotIsNotDedicated());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParkingSpotIsNotDedicated_ShouldReturnParkingSpotIsNotDedicatedError()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Regular spot"),
            state: ParkingSpotState.Active); // Not dedicated

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotIsNotDedicated());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParkingSpotIsAlreadyAssigned_ShouldReturnParkingSpotIsAlreadyAssignedError()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var existingManagerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Dedicated spot"),
            managerId: existingManagerId, // Already assigned
            state: ParkingSpotState.Dedicated);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotIsAlreadyAssigned());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenManagerDoesNotExist_ShouldReturnManagerNotFoundError()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Dedicated spot"),
            state: ParkingSpotState.Dedicated);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _userRepository.GetByIdAsync(managerId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.ManagerNotFound());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _userRepository.DidNotReceive().Update(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsNotBusinessManager_ShouldReturnManagerNotFoundError()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Dedicated spot"),
            state: ParkingSpotState.Dedicated);

        var regularUser = User.Create(
            firstName: UserName.Create("John"),
            lastName: UserName.Create("Doe"),
            email: Email.Create("john.doe@example.com"),
            phone: Phone.Create("123456789"),
            password: Password.Create("asdasdasd"),
            role: UserRole.Employee); // Not a business manager

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _userRepository.GetByIdAsync(managerId, Arg.Any<CancellationToken>())
            .Returns(regularUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.ManagerNotFound());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _userRepository.DidNotReceive().Update(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenManagerIsAlreadyAssignedToParkingSpot_ShouldReturnManagerIsAlreadyAssignedError()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var existingParkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Dedicated spot"),
            state: ParkingSpotState.Dedicated);

        var manager = User.Create(
            firstName: UserName.Create("John"),
            lastName: UserName.Create("Doe"),
            email: Email.Create("john.doe@example.com"),
            phone: Phone.Create("123456789"),
            password: Password.Create("asdasdasd"),
            role: UserRole.BusinessManager);

        // Manager already has a parking spot assigned
        manager.AssignParkingSpot(existingParkingSpotId);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _userRepository.GetByIdAsync(managerId, Arg.Any<CancellationToken>())
            .Returns(manager);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.ManagerIsAlreadyAssignedToParkingSpot());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _userRepository.DidNotReceive().Update(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllConditionsAreMet_ShouldAssignParkingSpotToManager()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            id: parkingSpotId,
            name: ParkingSpotName.Create("A1"),
            description: ParkingSpotDescription.Create("Dedicated spot"),
            state: ParkingSpotState.Dedicated);

        var manager = User.Create(
            firstName: UserName.Create("John"),
            lastName: UserName.Create("Doe"),
            email: Email.Create("john.doe@example.com"),
            phone: Phone.Create("123456789"),
            password: Password.Create("asdasdasd"),
            role: UserRole.BusinessManager);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);
        _userRepository.GetByIdAsync(managerId, Arg.Any<CancellationToken>())
            .Returns(manager);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        manager.AssignedParkingSpotId.Should().Be(parkingSpotId);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _userRepository.Received(1).Update(manager);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

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
    public async Task Handle_WhenParkingSpotIsDeactivated_ShouldReturnParkingSpotIsNotDedicatedError()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var parkingSpotId = Guid.NewGuid();
        var command = new AssignDedicatedParkingSpotCommand(managerId, parkingSpotId);

        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create("A1"),
            ParkingSpotDescription.Create("Deactivated spot"),
            state: ParkingSpotState.Deactivated);

        _parkingSpotsRepository.GetByIdAsync(parkingSpotId, Arg.Any<CancellationToken>())
            .Returns(parkingSpot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ParkingSpotErrors.ParkingSpotIsNotDedicated());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}