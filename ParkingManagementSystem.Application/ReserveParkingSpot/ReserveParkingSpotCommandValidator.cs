using FluentValidation;

namespace ParkingManagementSystem.Application.ReserveParkingSpot;

public class ReserveParkingSpotCommandValidator : AbstractValidator<ReserveParkingSpotCommand>
{
    public ReserveParkingSpotCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required");

        RuleFor(x => x.Dates)
            .NotEmpty().WithMessage("At least one date is required.")
            .Must(dates => dates.All(d => d >= DateOnly.FromDateTime(DateTime.Now)))
            .WithMessage("Dates cannot be in the past.")
            .Must(dates => dates.Distinct().Count() == dates.Count)
            .WithMessage("Dates must be unique.")
            .Must(dates => dates.All(d => d <= DateOnly.FromDateTime(DateTime.Now.AddDays(14))))
            .WithMessage("Dates cannot be more than 2 weeks in the future.");
    }
}