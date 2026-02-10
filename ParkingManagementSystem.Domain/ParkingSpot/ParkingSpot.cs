using ParkingManagementSystem.Domain.ParkingSpot.Enums;
using ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;
using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.ParkingSpot;

public sealed class ParkingSpot : AggregateRoot
{
    public ParkingSpotName Name { get; }
    public ParkingSpotState State { get; }
    public ParkingSpotDescription Description { get; }
    public Guid? ManagerId { get; }

    private ParkingSpot(
        Guid id,
        ParkingSpotName name,
        ParkingSpotState state,
        ParkingSpotDescription description, Guid? managerId) : base(id)
    {
        Name = name;
        State = state;
        Description = description;
        ManagerId = managerId;
    }

    private ParkingSpot(Guid id) : base(id)
    {
    }

    public static ParkingSpot Create(
        ParkingSpotName name,
        ParkingSpotDescription description,
        ParkingSpotState state = ParkingSpotState.Active,
        Guid? managerId = null,
        Guid? id = null)
    {
        var parkingSpotId = id ?? Guid.NewGuid();

        return new ParkingSpot(
            id: parkingSpotId,
            name: name,
            state: state,
            description: description,
            managerId: managerId);
    }
}