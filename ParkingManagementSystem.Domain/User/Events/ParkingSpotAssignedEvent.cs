using ParkingManagementSystem.Domain.Common.Interfaces;

namespace ParkingManagementSystem.Domain.User.Events;

public record ParkingSpotAssignedEvent(Guid ManagerId, Guid ParkingSpotId) : IDomainEvent;