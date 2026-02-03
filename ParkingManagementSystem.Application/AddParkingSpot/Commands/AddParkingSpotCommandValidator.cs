using FluentValidation;

namespace ParkingManagementSystem.Application.AddParkingSpot.Commands;

public class AddParkingSpotCommandValidator : AbstractValidator<AddParkingSpotCommand>
{
    public AddParkingSpotCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters long")
            .MaximumLength(20).WithMessage("Name must be at most 20 characters long");
        
        RuleFor(x => x.Description)
            .MaximumLength(150).WithMessage("Description must be at most 150 characters long");
        
        RuleFor(x => x.State)
            .IsInEnum().WithMessage("State is invalid");
    }
}