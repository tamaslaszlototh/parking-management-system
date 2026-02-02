using ParkingManagementSystem.Domain.Common.Interfaces;

namespace ParkingManagementSystem.Domain.Common;

public abstract class AggregateRoot : Entity
{
    protected readonly List<IDomainEvent> DomainEvents = [];

    protected AggregateRoot(Guid id) : base(id)
    {
    }

    public List<IDomainEvent> PopDomainEvents()
    {
        var domainEventsCopy = DomainEvents.ToList();
        DomainEvents.Clear();
        return domainEventsCopy;
    }
}