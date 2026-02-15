using FluentValidation;

namespace ParkingManagementSystem.Application.RemoveDedicatedParkingSpotAssignment;

public class RemoveDedicatedParkingSpotAssignmentCommandValidator
    : AbstractValidator<RemoveDedicatedParkingSpotAssignmentCommand>
{
    public RemoveDedicatedParkingSpotAssignmentCommandValidator()
    {
        RuleFor(x => x.ParkingSpotId)
            .NotEmpty().WithMessage(x => $"Parking spot id is required");
    }
}