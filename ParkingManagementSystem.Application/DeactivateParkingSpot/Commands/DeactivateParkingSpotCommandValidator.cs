using FluentValidation;

namespace ParkingManagementSystem.Application.DeactivateParkingSpot.Commands;

public class DeactivateParkingSpotCommandValidator : AbstractValidator<DeactivateParkingSpotCommand>
{
    public DeactivateParkingSpotCommandValidator()
    {
        RuleFor(x => x.ParkingSpotId)
            .NotEmpty().WithMessage("Parking spot id is required");
    }
}