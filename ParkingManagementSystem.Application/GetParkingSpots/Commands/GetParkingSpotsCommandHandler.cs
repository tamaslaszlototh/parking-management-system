using MediatR;
using ParkingManagementSystem.Application.GetParkingSpots.Models;
using ErrorOr;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot;

namespace ParkingManagementSystem.Application.GetParkingSpots.Commands;

public class GetParkingSpotsCommandHandler : IRequestHandler<GetParkingSpotsCommand, ErrorOr<GetParkingSpotsResult>>
{
    private readonly IParkingSpotsRepository _parkingSpotsRepository;

    public GetParkingSpotsCommandHandler(IParkingSpotsRepository parkingSpotsRepository)
    {
        _parkingSpotsRepository = parkingSpotsRepository;
    }

    public async Task<ErrorOr<GetParkingSpotsResult>> Handle(GetParkingSpotsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            List<ParkingSpot> parkingSpots;

            if (request.IncludeDeactivated)
            {
                parkingSpots = await _parkingSpotsRepository.GetParkingSpotsAsync(cancellationToken);
            }
            else
            {
                parkingSpots = await _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(cancellationToken);
            }

            var parkingSpotDtos =
                parkingSpots.ConvertAll(p => new ParkingSpotDto(p.Id, p.Name.Value, p.Description?.Value, p.State));
            return new GetParkingSpotsResult(parkingSpotDtos);
        }
        catch
        {
            return Error.Failure();
        }
    }
}