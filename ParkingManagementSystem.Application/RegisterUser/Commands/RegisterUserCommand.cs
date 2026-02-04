using ErrorOr;
using MediatR;
using ParkingManagementSystem.Domain.User;
using ParkingManagementSystem.Domain.User.Enums;

namespace ParkingManagementSystem.Application.RegisterUser.Commands;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password,
    UserRole Role) : IRequest<ErrorOr<User>>;