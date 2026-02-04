using FluentValidation;

namespace ParkingManagementSystem.Application.RegisterUser.Commands;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(u => u.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters long")
            .MaximumLength(20).WithMessage("First name must be at most 20 characters long");

        RuleFor(u => u.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters long")
            .MaximumLength(20).WithMessage("Last name must be at most 20 characters long");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required")
            .MaximumLength(255).WithMessage("Email must be at most 255 characters long")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(u => u.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(50).WithMessage("Phone must be at most 50 characters long");
    }
}