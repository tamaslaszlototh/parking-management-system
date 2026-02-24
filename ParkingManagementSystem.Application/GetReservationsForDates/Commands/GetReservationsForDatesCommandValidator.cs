using FluentValidation;

namespace ParkingManagementSystem.Application.GetReservationsForDates.Commands;

public class GetReservationsForDatesCommandValidator : AbstractValidator<GetReservationsForDatesCommand>
{
    public GetReservationsForDatesCommandValidator()
    {
        RuleFor(x => x.Dates)
            .NotEmpty().WithMessage(x => "Dates are required");
    }
}