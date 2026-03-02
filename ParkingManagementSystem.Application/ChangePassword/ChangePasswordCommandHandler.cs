using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.Common.Services;
using ParkingManagementSystem.Domain.User.ValueObjects;
using UserErrors = ParkingManagementSystem.Domain.User.Errors.Errors.User;

namespace ParkingManagementSystem.Application.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ErrorOr<Success>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                return UserErrors.UserNotFound();
            }

            var passwordVerificationResult = _passwordService.Verify(request.CurrentPassword, user.Password.Value);

            if (!passwordVerificationResult)
            {
                return UserErrors.PasswordIsInvalid();
            }

            var hashedNewPassword = _passwordService.Hash(request.NewPassword);
            user.ChangePassword(Password.Create(hashedNewPassword));

            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success;
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Error.Failure();
        }
    }
}