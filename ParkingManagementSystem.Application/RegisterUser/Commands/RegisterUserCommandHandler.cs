using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.Common.Services;
using ParkingManagementSystem.Domain.User;
using ParkingManagementSystem.Domain.User.ValueObjects;

namespace ParkingManagementSystem.Application.RegisterUser.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<User>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<ErrorOr<User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is not null)
        {
            return Error.Conflict(description: "User already exists");
        }

        var hashedPassword = _passwordService.Hash(request.Password);

        var newUser = User.Create(
            firstName: UserName.Create(request.FirstName),
            lastName: UserName.Create(request.LastName),
            email: Email.Create(request.Email),
            password: Password.Create(hashedPassword),
            phone: Phone.Create(request.Phone),
            role: request.Role
        );

        await _userRepository.AddAsync(newUser, cancellationToken);
        
        return newUser;
    }
}