using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.Common.Services;
using ParkingManagementSystem.Application.LoginUser.Models;
using ParkingManagementSystem.Domain.User.ValueObjects;

namespace ParkingManagementSystem.Application.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ErrorOr<LoginUserResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(IUserRepository userRepository, IPasswordService passwordService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ErrorOr<LoginUserResult>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
            return Error.Unauthorized();

        var passwordVerificationResult = _passwordService.Verify(request.Password, user.Password.Value);

        if (!passwordVerificationResult)
            return Error.Unauthorized();

        var authToken = _jwtTokenGenerator.GenerateToken(user);

        return new LoginUserResult(authToken, user);
    }
}