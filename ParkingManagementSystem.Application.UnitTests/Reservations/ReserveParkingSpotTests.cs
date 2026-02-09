using ErrorOr;
using FluentAssertions;
using NSubstitute;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.ReserveParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot;
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
    }

    [Fact]
    public async Task Handle_WhenNoFreeParkingSpotAvailable_ShouldReturnNotFoundFreeParkingSpotError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

        var parkingSpots = new List<ParkingSpot>
        {
            ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1 description")),
            ParkingSpot.Create(ParkingSpotName.Create("A2"), ParkingSpotDescription.Create("A2 description"))
        };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _parkingSpotsRepository.FreeForReservationFor(date, userId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ReservationErrors.NotFoundFreeParkingSpot(date));
    }

    [Fact]
    public async Task Handle_WhenUserHasNoReservationForDate_ShouldReserveParkingSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

        var parkingSpot =
            ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1 description"));
        var parkingSpots = new List<ParkingSpot> { parkingSpot };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _parkingSpotsRepository.FreeForReservationFor(date, userId, parkingSpot.Id, Arg.Any<CancellationToken>())
            .Returns(true);

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
    public async Task Handle_WhenFirstParkingSpotIsNotFree_ShouldCheckNextParkingSpot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var command = new ReserveParkingSpotCommand(userId, date);

        var parkingSpot1 =
            ParkingSpot.Create(ParkingSpotName.Create("A1"), ParkingSpotDescription.Create("A1 description"));
        var parkingSpot2 =
            ParkingSpot.Create(ParkingSpotName.Create("A2"), ParkingSpotDescription.Create("A2 description"));
        var parkingSpots = new List<ParkingSpot> { parkingSpot1, parkingSpot2 };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.HasReservationForAsync(userId, date, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(Arg.Any<CancellationToken>())
            .Returns(parkingSpots);
        _parkingSpotsRepository.FreeForReservationFor(date, userId, parkingSpot1.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _parkingSpotsRepository.FreeForReservationFor(date, userId, parkingSpot2.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        await _parkingSpotsRepository.Received(1).FreeForReservationFor(
            date, userId, parkingSpot1.Id, Arg.Any<CancellationToken>());
        await _parkingSpotsRepository.Received(1).FreeForReservationFor(
            date, userId, parkingSpot2.Id, Arg.Any<CancellationToken>());

        await _reservationsRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.ParkingSpotId == parkingSpot2.Id),
            Arg.Any<CancellationToken>());
    }
}