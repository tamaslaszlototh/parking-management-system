using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.Common.Persistence.Interfaces;
using ParkingManagementSystem.Domain.ParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;

namespace ParkingManagementSystem.Application.AddParkingSpot.Commands;

public class AddParkingSpotCommandHandler : IRequestHandler<AddParkingSpotCommand, ErrorOr<ParkingSpot>>
{
    private readonly IParkingSpotsRepository _parkingSpotsRepository;

    public AddParkingSpotCommandHandler(IParkingSpotsRepository parkingSpotsRepository)
    {
        _parkingSpotsRepository = parkingSpotsRepository;
    }

    public async Task<ErrorOr<ParkingSpot>> Handle(AddParkingSpotCommand request,
        CancellationToken cancellationToken)
    {
        var parkingSpot = ParkingSpot.Create(
            ParkingSpotName.Create(request.Name),
            ParkingSpotDescription.Create(request.Description),
            request.State);

        await _parkingSpotsRepository.AddAsync(parkingSpot);
        return parkingSpot;
    }
}