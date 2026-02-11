using ErrorOr;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ParkingManagementSystem.Application.CancelReservation;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.Reservation;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;
using ReservationErrors = ParkingManagementSystem.Domain.Reservation.Errors.Errors.Reservation;

namespace ParkingManagementSystem.Application.UnitTests.Reservations;

public class CancelReservationTests
{
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CancelReservationCommandHandler _handler;

    public CancelReservationTests()
    {
        _reservationsRepository = Substitute.For<IReservationsRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CancelReservationCommandHandler(
            _reservationsRepository,
            _unitOfWork,
            _userRepository);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnUserNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservationIds = new List<Guid> { Guid.NewGuid() };
        var command = new CancelReservationCommand(reservationIds, userId);

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.UserNotFound());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _reservationsRepository.DidNotReceive()
            .GetReservationsByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReservationNotFound_ShouldReturnReservationNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservationId1 = Guid.NewGuid();
        var reservationId2 = Guid.NewGuid();
        var reservationIds = new List<Guid> { reservationId1, reservationId2 };
        var command = new CancelReservationCommand(reservationIds, userId);

        var date = DateOnly.FromDateTime(DateTime.Now);
        var parkingSpotId = Guid.NewGuid();
        var reservation = Reservation.Create(userId, parkingSpotId, date);
        var foundReservations = new List<Reservation> { reservation }; // Only 1 out of 2 found

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.GetReservationsByIdsAsync(reservationIds, Arg.Any<CancellationToken>())
            .Returns(foundReservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ReservationErrors.ReservationNotFound());

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _reservationsRepository.DidNotReceive().Remove(Arg.Any<Reservation>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReservationBelongsToAnotherUser_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var reservationIds = new List<Guid> { reservationId };
        var command = new CancelReservationCommand(reservationIds, userId);

        var date = DateOnly.FromDateTime(DateTime.Now);
        var parkingSpotId = Guid.NewGuid();
        var reservation = Reservation.Create(otherUserId, parkingSpotId, date); // Belongs to different user
        var reservations = new List<Reservation> { reservation };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.GetReservationsByIdsAsync(reservationIds, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _reservationsRepository.DidNotReceive().Remove(Arg.Any<Reservation>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSingleReservationIsValid_ShouldCancelReservation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var reservationIds = new List<Guid> { reservationId };
        var command = new CancelReservationCommand(reservationIds, userId);

        var date = DateOnly.FromDateTime(DateTime.Now);
        var parkingSpotId = Guid.NewGuid();
        var reservation = Reservation.Create(userId, parkingSpotId, date);
        var reservations = new List<Reservation> { reservation };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.GetReservationsByIdsAsync(reservationIds, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.Should().Contain(date);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _reservationsRepository.Received(1).Remove(reservation);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMultipleReservationsAreValid_ShouldCancelAllReservations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservationId1 = Guid.NewGuid();
        var reservationId2 = Guid.NewGuid();
        var reservationId3 = Guid.NewGuid();
        var reservationIds = new List<Guid> { reservationId1, reservationId2, reservationId3 };
        var command = new CancelReservationCommand(reservationIds, userId);

        var date1 = DateOnly.FromDateTime(DateTime.Now);
        var date2 = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var date3 = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
        var parkingSpotId = Guid.NewGuid();

        var reservation1 = Reservation.Create(userId, parkingSpotId, date1);
        var reservation2 = Reservation.Create(userId, parkingSpotId, date2);
        var reservation3 = Reservation.Create(userId, parkingSpotId, date3);
        var reservations = new List<Reservation> { reservation1, reservation2, reservation3 };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.GetReservationsByIdsAsync(reservationIds, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(new[] { date1, date2, date3 });

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _reservationsRepository.Received(1).Remove(reservation1);
        _reservationsRepository.Received(1).Remove(reservation2);
        _reservationsRepository.Received(1).Remove(reservation3);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSomeReservationsBelongToOtherUser_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var reservationId1 = Guid.NewGuid();
        var reservationId2 = Guid.NewGuid();
        var reservationIds = new List<Guid> { reservationId1, reservationId2 };
        var command = new CancelReservationCommand(reservationIds, userId);

        var date1 = DateOnly.FromDateTime(DateTime.Now);
        var date2 = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var parkingSpotId = Guid.NewGuid();

        var reservation1 = Reservation.Create(userId, parkingSpotId, date1); // User's reservation
        var reservation2 = Reservation.Create(otherUserId, parkingSpotId, date2); // Other user's reservation
        var reservations = new List<Reservation> { reservation1, reservation2 };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.GetReservationsByIdsAsync(reservationIds, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        _reservationsRepository.DidNotReceive().Remove(Arg.Any<Reservation>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservationIds = new List<Guid> { Guid.NewGuid() };
        var command = new CancelReservationCommand(reservationIds, userId);

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
    public async Task Handle_WhenReservationsFoundButCountMismatch_ShouldNotCallRemove()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservationId1 = Guid.NewGuid();
        var reservationId2 = Guid.NewGuid();
        var reservationId3 = Guid.NewGuid();
        var reservationIds = new List<Guid> { reservationId1, reservationId2, reservationId3 };
        var command = new CancelReservationCommand(reservationIds, userId);

        var date = DateOnly.FromDateTime(DateTime.Now);
        var parkingSpotId = Guid.NewGuid();
        var reservation = Reservation.Create(userId, parkingSpotId, date);
        var foundReservations = new List<Reservation> { reservation }; // Only 1 out of 3 found

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.GetReservationsByIdsAsync(reservationIds, Arg.Any<CancellationToken>())
            .Returns(foundReservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();

        _reservationsRepository.DidNotReceive().Remove(Arg.Any<Reservation>());
    }

    [Fact]
    public async Task Handle_WhenCancellingReservations_ShouldReturnCorrectDatesInOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservationId1 = Guid.NewGuid();
        var reservationId2 = Guid.NewGuid();
        var reservationIds = new List<Guid> { reservationId1, reservationId2 };
        var command = new CancelReservationCommand(reservationIds, userId);

        var date1 = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
        var date2 = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
        var parkingSpotId = Guid.NewGuid();

        var reservation1 = Reservation.Create(userId, parkingSpotId, date1);
        var reservation2 = Reservation.Create(userId, parkingSpotId, date2);
        var reservations = new List<Reservation> { reservation1, reservation2 };

        _userRepository.ExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _reservationsRepository.GetReservationsByIdsAsync(reservationIds, Arg.Any<CancellationToken>())
            .Returns(reservations);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value[0].Should().Be(date1);
        result.Value[1].Should().Be(date2);
    }
}