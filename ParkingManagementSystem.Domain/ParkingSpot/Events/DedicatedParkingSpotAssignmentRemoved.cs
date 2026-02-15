using ParkingManagementSystem.Domain.Common.Interfaces;

namespace ParkingManagementSystem.Domain.ParkingSpot.Events;

public record DedicatedParkingSpotAssignmentRemoved(Guid ParkingSpotId) : IDomainEvent;