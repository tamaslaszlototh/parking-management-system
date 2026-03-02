using ErrorOr;
using FluentAssertions;
using NSubstitute;
using ParkingManagementSystem.Application.ChangePassword;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.Common.Services;
using ParkingManagementSystem.Domain.User;
using ParkingManagementSystem.Domain.User.Enums;
using ParkingManagementSystem.Domain.User.ValueObjects;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;

namespace ParkingManagementSystem.Application.UnitTests.Users;

public class ChangePasswordTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IPasswordService _passwordServiceMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _passwordServiceMock = Substitute.For<IPasswordService>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _sut = new ChangePasswordCommandHandler(_userRepositoryMock, _passwordServiceMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUserNotFoundError()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "current", "new");
        _userRepositoryMock.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>()).Returns((User)null!);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(UserErrors.UserNotFound());
        await _unitOfWorkMock.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidCurrentPassword_ReturnsPasswordIsInvalidError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var command = new ChangePasswordCommand(userId, "wrong-password", "new-password");

        _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordServiceMock.Verify("wrong-password", user.Password.Value).Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(UserErrors.PasswordIsInvalid());
        await _unitOfWorkMock.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
public async Task Handle_ValidRequest_SuccessfullyChangesPassword()
{
    // Arrange
    var userId = Guid.NewGuid();
    var currentPasswordPlain = "current123!";
    var currentPasswordHash = "$2a$12$test_hash_for_unit_tests";
    var newPasswordPlain = "newPassword123!";
    var newPasswordHash = "$2a$13$test_hash_for_unit_tests";

    var user = CreateTestUser(userId, currentPasswordHash);
    var command = new ChangePasswordCommand(userId, currentPasswordPlain, newPasswordPlain);

    _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
    _passwordServiceMock.Verify(currentPasswordPlain, currentPasswordHash).Returns(true);
    _passwordServiceMock.Hash(newPasswordPlain).Returns(newPasswordHash);
    
    // JAVÍTOTT: Task.FromResult vagy async mock
    _unitOfWorkMock.BeginTransactionAsync(Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(0)); // vagy Returns(Task.CompletedTask) ha void Task
    _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(0)); // vagy Returns(Task.CompletedTask) ha void Task
    _unitOfWorkMock.CommitTransactionAsync(Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(0)); // vagy Returns(Task.CompletedTask) ha void Task

    // Act
    var result = await _sut.Handle(command, CancellationToken.None);

    // Assert
    result.IsError.Should().BeFalse();
    result.Value.Should().Be(Result.Success);
    user.Password.Value.Should().Be(newPasswordHash);
    user.FirstName.Value.Should().Be("John");
    _userRepositoryMock.Received(1).Update(user);
    await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    await _unitOfWorkMock.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    _passwordServiceMock.Received(1).Verify(currentPasswordPlain, currentPasswordHash);
    _passwordServiceMock.Received(1).Hash(newPasswordPlain);
}


    [Fact]
    public async Task Handle_EmptyPassword_ThrowsFromPasswordService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var command = new ChangePasswordCommand(userId, "", "new");

        _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordServiceMock.Verify("", user.Password.Value)
            .ReturnsForAnyArgs(x => throw new ArgumentException("Password cannot be empty"));

        // Act & Assert
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        await _unitOfWorkMock.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DatabaseException_RollsbackTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var command = new ChangePasswordCommand(userId, "current", "new");

        _userRepositoryMock.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordServiceMock.Verify("current", user.Password.Value).Returns(true);
        _passwordServiceMock.Hash("new").Returns("new_hash");

        _unitOfWorkMock.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs<int>(x => throw new InvalidOperationException("Database timeout"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _unitOfWorkMock.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    private static User CreateTestUser(Guid userId, string passwordHash = "$2a$12$test_hash_for_unit_tests")
    {
        var user = User.Create(
            firstName: UserName.Create("John"),
            lastName: UserName.Create("Doe"),
            email: Email.Create("john.doe@example.com"),
            phone: Phone.Create("+36301234567"),
            password: Password.Create(passwordHash),
            id: userId,
            role: UserRole.Employee
        );
        
        return user;
    }
}