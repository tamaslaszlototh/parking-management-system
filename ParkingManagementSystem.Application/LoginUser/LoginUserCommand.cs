using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.LoginUser.Models;
using ParkingManagementSystem.Domain.User;

namespace ParkingManagementSystem.Application.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<ErrorOr<LoginUserResult>>;