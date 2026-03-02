using FluentValidation;

namespace ParkingManagementSystem.Application.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(u => u.UserId)
            .NotEmpty().WithMessage("User Id is required");

        RuleFor(u => u.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required")
            .MinimumLength(8).WithMessage("Current password must be at least 8 characters long")
            .MaximumLength(128).WithMessage("Current password cannot exceed 128 characters")
            .Matches(@"[A-Z]").WithMessage("Current password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Current password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Current password must contain at least one digit")
            .Matches(@"[!@#$%^&*(),.?""':{}|/<>]")
            .WithMessage("Current password must contain at least one special character");

        RuleFor(u => u.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
            .MaximumLength(128).WithMessage("New password cannot exceed 128 characters")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("New password must contain at least one digit")
            .Matches(@"[!@#$%^&*(),.?""':{}|/<>]")
            .WithMessage("New password must contain at least one special character");
    }
}