using MediatR;
using ParkingManagementSystem.Application.GetParkingSpots.Models;
using ErrorOr;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;

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
            var parkingSpots = await _parkingSpotsRepository.GetNotDeactivatedParkingSpotsAsync(cancellationToken);
            var parkingSpotDtos =
                parkingSpots.ConvertAll(p => new ParkingSpotDto(p.Id, p.Name.Value, p.Description?.Value));
            return new GetParkingSpotsResult(parkingSpotDtos);
        }
        catch(Exception ex)
        {
            return Error.Failure();
        }
    }
}