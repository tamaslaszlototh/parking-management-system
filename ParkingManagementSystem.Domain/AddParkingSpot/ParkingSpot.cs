using ParkingManagementSystem.Domain.AddParkingSpot.Enums;
using ParkingManagementSystem.Domain.AddParkingSpot.ValueObjects;
using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.AddParkingSpot;

public sealed class ParkingSpot : AggregateRoot
{
    public ParkingSpotName Name { get; }
    public ParkingSpotState State { get; }
    public ParkingSpotDescription Description { get; }

    private ParkingSpot(
        Guid id,
        ParkingSpotName name,
        ParkingSpotState state,
        ParkingSpotDescription description) : base(id)
    {
        Name = name;
        State = state;
        Description = description;
    }

    public static ParkingSpot Create(
        ParkingSpotName name,
        ParkingSpotDescription description,
        ParkingSpotState state = ParkingSpotState.Active,
        Guid? id = null)
    {
        var parkingSpotId = id ?? Guid.NewGuid();

        return new ParkingSpot(
            id: parkingSpotId,
            name: name,
            state: state,
            description: description);
    }
}