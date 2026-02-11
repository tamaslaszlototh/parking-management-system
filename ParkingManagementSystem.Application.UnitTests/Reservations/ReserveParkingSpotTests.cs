using ErrorOr;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.ReserveParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;
using ParkingManagementSystem.Domain.Reservation;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;
using ReservationErrors = ParkingManagementSystem.Domain.Reservation.Errors.Errors.Reservation;

namespace ParkingManagementSystem.Application.UnitTests.Reservations;

public class ReserveParkingSpotTests
{
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IParkingSpotsRepository _parkingSpotsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReserveParkingSpotCommandHandler _handler;

    public ReserveParkingSpotTests()
    {
        _reservationsRepository = Substitute.For<IReservationsRepository>();
        _parkingSpotsRepository = Substitute.For<IParkingSpotsRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ReserveParkingSpotCommandHandler(
            _reservationsRepository,
            _parkingSpotsRepository,
            _userRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnUserNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dates = new List<DateOnly> { DateOnly.FromDateTime(DateTime.Now) };
        var command = new ReserveParkingSpotCommand(userId, dates);

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.UserNotFound());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _reservationsRepository.DidNotReceive()
            .HasReservationForAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasReservationForDate_ShouldReturnUserAlreadyHasReservationError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var dates = new List<DateOnly> { date };
        var command = new ReserveParkingSpotCommand(userId, dates);

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("UserAlreadyHasReservationForDates");

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _parkingSpotsRepository.DidNotReceive()
            .GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllParkingSpotsAreReserved_ShouldReturnNotFoundFreeParkingSpotError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var dates = new List<DateOnly> { date };
        var command = new ReserveParkingSpotCommand(userId, dates);

        var parkingSpot1 = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"));
        var parkingSpot2 = ParkingSpot.Create(ParkingSpotName.Create("A2"), ParkingSpotDescription.Create("A2"));
        var parkingSpots = new List<ParkingSpot> { parkingSpot1, parkingSpot2 };
        var reservedParkingSpotIds = new List<Guid> { parkingSpot1.Id, parkingSpot2.Id };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("NotFoundFreeParkingSpotForDates");

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFreeParkingSpotExists_ShouldReserveParkingSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var dates = new List<DateOnly> { date };
        var command = new ReserveParkingSpotCommand(userId, dates);

        var parkingSpot = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"));
        var parkingSpots = new List<ParkingSpot> { parkingSpot };
        var reservedParkingSpotIds = new List<Guid>();

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r =>
                r.UserId == userId &&
                r.ParkingSpotId == parkingSpot.Id &&
                r.ReservationDate == date),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSomeParkingSpotsAreReserved_ShouldReserveFirstAvailableSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var dates = new List<DateOnly> { date };
        var command = new ReserveParkingSpotCommand(userId, dates);

        var parkingSpot1 = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"));
        var parkingSpot2 = ParkingSpot.Create(ParkingSpotName.Create("A2"), ParkingSpotDescription.Create("A2"));
        var parkingSpot3 = ParkingSpot.Create(ParkingSpotName.Create("A3"), ParkingSpotDescription.Create("A3"));
        var parkingSpots = new List<ParkingSpot> { parkingSpot1, parkingSpot2, parkingSpot3 };
        var reservedParkingSpotIds = new List<Guid> { parkingSpot1.Id, parkingSpot3.Id };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.ParkingSpotId == parkingSpot2.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDedicatedSpotExistsAndUserIsNotManager_ShouldSkipDedicatedSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var dates = new List<DateOnly> { date };
        var command = new ReserveParkingSpotCommand(userId, dates);

        var dedicatedSpot = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"),
            managerId: managerId, state: ParkingSpotState.Dedicated);
        var regularSpot = ParkingSpot.Create(ParkingSpotName.Create("A2"), ParkingSpotDescription.Create("A2"));
        var parkingSpots = new List<ParkingSpot> { dedicatedSpot, regularSpot };
        var reservedParkingSpotIds = new List<Guid>();

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.ParkingSpotId == regularSpot.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDedicatedSpotExistsAndUserIsManager_ShouldReserveDedicatedSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var dates = new List<DateOnly> { date };
        var command = new ReserveParkingSpotCommand(userId, dates);

        var dedicatedSpot = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"),
            managerId: userId);
        var parkingSpots = new List<ParkingSpot> { dedicatedSpot };
        var reservedParkingSpotIds = new List<Guid>();

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.ParkingSpotId == dedicatedSpot.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOnlyDedicatedSpotsExistForOtherManagers_ShouldReturnNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherManagerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var dates = new List<DateOnly> { date };
        var command = new ReserveParkingSpotCommand(userId, dates);

        var dedicatedSpot1 = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"),
            managerId: otherManagerId, state: ParkingSpotState.Dedicated);
        var dedicatedSpot2 = ParkingSpot.Create(ParkingSpotName.Create("A2"), ParkingSpotDescription.Create("A2"),
            managerId: otherManagerId, state: ParkingSpotState.Dedicated);
        var parkingSpots = new List<ParkingSpot> { dedicatedSpot1, dedicatedSpot2 };
        var reservedParkingSpotIds = new List<Guid>();

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("NotFoundFreeParkingSpotForDates");

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dates = new List<DateOnly> { DateOnly.FromDateTime(DateTime.Now) };
        var command = new ReserveParkingSpotCommand(userId, dates);

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
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
    public async Task Handle_WhenReservingMultipleDates_ShouldReserveAllDates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date1 = DateOnly.FromDateTime(DateTime.Now);
        var date2 = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var date3 = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
        var dates = new List<DateOnly> { date1, date2, date3 };
        var command = new ReserveParkingSpotCommand(userId, dates);

        var parkingSpot1 = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"));
        var parkingSpot2 = ParkingSpot.Create(ParkingSpotName.Create("A2"), ParkingSpotDescription.Create("A2"));
        var parkingSpots = new List<ParkingSpot> { parkingSpot1, parkingSpot2 };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<Guid>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _reservationsRepository.Received(3).AddAsync(
            Arg.Any<Reservation>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserHasReservationForSomeDates_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date1 = DateOnly.FromDateTime(DateTime.Now);
        var date2 = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var dates = new List<DateOnly> { date1, date2 };
        var command = new ReserveParkingSpotCommand(userId, dates);

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date1, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date2, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("UserAlreadyHasReservationForDates");

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _parkingSpotsRepository.DidNotReceive()
            .GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoFreeSpotsForSomeDates_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date1 = DateOnly.FromDateTime(DateTime.Now);
        var date2 = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var dates = new List<DateOnly> { date1, date2 };
        var command = new ReserveParkingSpotCommand(userId, dates);

        var parkingSpot = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"));
        var parkingSpots = new List<ParkingSpot> { parkingSpot };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);

        // First date has free spot, second date is fully booked
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(date1, Arg.Any<CancellationToken>())
            .Returns(new List<Guid>());
        _reservationsRepository.GetReservedParkingSpotsForDateAsync(date2, Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { parkingSpot.Id });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("NotFoundFreeParkingSpotForDates");

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }
}