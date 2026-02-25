using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.GetReservationsForUser.Models;

namespace ParkingManagementSystem.Application.GetReservationsForUser.Commands;

public class
    GetReservationsForUserCommandHandler : IRequestHandler<GetReservationsForUserCommand,
    ErrorOr<GetReservationsForUserResult>>
{
    private readonly IReservationsRepository _reservationsRepository;

    public GetReservationsForUserCommandHandler(IReservationsRepository reservationsRepository)
    {
        _reservationsRepository = reservationsRepository;
    }

    public async Task<ErrorOr<GetReservationsForUserResult>> Handle(GetReservationsForUserCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var reservations =
                await _reservationsRepository.GetReservationsForUserAsync(request.UserId, request.IncludeExpired,
                    cancellationToken);

            return new GetReservationsForUserResult(reservations);
        }
        catch
        {
            return Error.Failure();
        }
    }
}