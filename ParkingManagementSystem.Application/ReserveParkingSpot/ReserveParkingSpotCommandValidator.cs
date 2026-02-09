using FluentValidation;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public class ReserveParkingSpotCommandValidator : AbstractValidator<ReserveParkingSpotCommand>
{
    public ReserveParkingSpotCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required");
        
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Now)).WithMessage("Date cannot be in the past.");
    }
}