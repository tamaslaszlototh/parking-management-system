using ErrorOr;
using MediatR;
using ParkingManagementSystem.Domain.User;

namespace ParkingManagementSystem.Application.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<ErrorOr<string>>;