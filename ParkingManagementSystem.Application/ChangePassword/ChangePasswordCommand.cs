using ErrorOr;
using MediatR;

namespace ParkingManagementSystem.Application.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : IRequest<ErrorOr<Success>>;