using FluentValidation;

namespace ParkingManagementSystem.Application.GetReservationsForUser.Commands;

public class GetReservationsForUserCommandValidator : AbstractValidator<GetReservationsForUserCommand>
{
    public GetReservationsForUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(x => "User id is required");
    }
}