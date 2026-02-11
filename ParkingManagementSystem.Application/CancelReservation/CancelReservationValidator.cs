using FluentValidation;

namespace ParkingManagementSystem.Application.CancelReservation;

public class CancelReservationValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required");

        RuleFor(x => x.ReservationIds)
            .NotEmpty().WithMessage("At least one reservation Id is required.");
    }
}