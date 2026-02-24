using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Application.GetReservationsForDates.Models;

namespace ParkingManagementSystem.Application.GetReservationsForDates.Commands;

public class
    GetReservationsForDatesCommandHandler : IRequestHandler<GetReservationsForDatesCommand,
    ErrorOr<GetReservationsForDatesResult>>
{
    private readonly IReservationsRepository _reservationsRepository;

    public GetReservationsForDatesCommandHandler(IReservationsRepository reservationsRepository)
    {
        _reservationsRepository = reservationsRepository;
    }

    public async Task<ErrorOr<GetReservationsForDatesResult>> Handle(GetReservationsForDatesCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var reservations =
                await _reservationsRepository.GetActiveReservationsForDatesAsync(request.Dates, cancellationToken);

            return new GetReservationsForDatesResult(reservations);
        }
        catch
        {
            return Error.Failure();
        }
    }
}