using FluentValidation;

namespace ParkingManagementSystem.Application.AssignDedicatedParkingSpot;

public class AssignDedicatedParkingSpotCommandValidator : AbstractValidator<AssignDedicatedParkingSpotCommand>
{
    public AssignDedicatedParkingSpotCommandValidator()
    {
        RuleFor(x => x.ParkingSpotId)
            .NotEmpty().WithMessage("Parking spot id is required");

        RuleFor(x => x.ManagerId)
            .NotEmpty().WithMessage("Manager id is required");
    }
}