using ErrorOr;
using FluentAssertions;
using NSubstitute;
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
    private readonly ReserveParkingSpotCommandHandler _handler;

    public ReserveParkingSpotTests()
    {
        _reservationsRepository = Substitute.For<IReservationsRepository>();
        _parkingSpotsRepository = Substitute.For<IParkingSpotsRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new ReserveParkingSpotCommandHandler(
            _reservationsRepository,
            _parkingSpotsRepository,
            _userRepository);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnUserNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.UserNotFound());

        await _reservationsRepository.DidNotReceive()
            .HasReservationForAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasReservationForDate_ShouldReturnUserAlreadyHasReservationError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ReservationErrors.UserAlreadyHasReservationForDate(date));

        await _parkingSpotsRepository.DidNotReceive()
            .GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllParkingSpotsAreReserved_ShouldReturnNotFoundFreeParkingSpotError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

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
        _reservationsRepository.GetReservedParkingSpotsForDate(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ReservationErrors.NotFoundFreeParkingSpot(date));
    }

    [Fact]
    public async Task Handle_WhenFreeParkingSpotExists_ShouldReserveParkingSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

        var parkingSpot = ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1"));
        var parkingSpots = new List<ParkingSpot> { parkingSpot };
        var reservedParkingSpotIds = new List<Guid>();

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _reservationsRepository.GetReservedParkingSpotsForDate(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r =>
                r.UserId == userId &&
                r.ParkingSpotId == parkingSpot.Id &&
                r.ReservationDate == date),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSomeParkingSpotsAreReserved_ShouldReserveFirstAvailableSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

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
        _reservationsRepository.GetReservedParkingSpotsForDate(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.ParkingSpotId == parkingSpot2.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDedicatedSpotExistsAndUserIsNotManager_ShouldSkipDedicatedSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

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
        _reservationsRepository.GetReservedParkingSpotsForDate(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.ParkingSpotId == regularSpot.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDedicatedSpotExistsAndUserIsManager_ShouldReserveDedicatedSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

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
        _reservationsRepository.GetReservedParkingSpotsForDate(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.ParkingSpotId == dedicatedSpot.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOnlyDedicatedSpotsExistForOtherManagers_ShouldReturnNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherManagerId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

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
        _reservationsRepository.GetReservedParkingSpotsForDate(date, Arg.Any<CancellationToken>())
            .Returns(reservedParkingSpotIds);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ReservationErrors.NotFoundFreeParkingSpot(date));
    }
}