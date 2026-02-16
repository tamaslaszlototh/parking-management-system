using ParkingManagementSystem.Domain.Common;
using ParkingManagementSystem.Domain.User.Enums;
using ParkingManagementSystem.Domain.User.Events;
using ParkingManagementSystem.Domain.User.ValueObjects;

namespace ParkingManagementSystem.Domain.User;

public sealed class User : AggregateRoot
{
    public UserName FirstName { get; }
    public UserName LastName { get; }
    public Email Email { get; }
    public Phone Phone { get; }
    public Password Password { get; }
    public UserRole Role { get; }
    public Guid? AssignedParkingSpotId { get; private set; }

    private User(
        Guid id,
        UserName firstName,
        UserName lastName,
        Email email,
        Phone phone,
        Password password,
        UserRole role,
        Guid? assignedParkingSpotId) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Password = password;
        Role = role;
        AssignedParkingSpotId = assignedParkingSpotId;
    }

    public static User Create(
        UserName firstName,
        UserName lastName,
        Email email,
        Phone phone,
        Password password,
        UserRole role = UserRole.Employee,
        Guid? id = null,
        Guid? assignedParkingSpotId = null
    )
    {
        var userId = id ?? Guid.NewGuid();

        return new User(
            id: userId,
            firstName: firstName,
            lastName: lastName,
            email: email,
            phone: phone,
            password: password,
            role: role,
            assignedParkingSpotId: assignedParkingSpotId);
    }

    public void AssignParkingSpot(Guid parkingSpotId)
    {
        AssignedParkingSpotId = parkingSpotId;
        DomainEvents.Add(new ParkingSpotAssignedEvent(Id, parkingSpotId));
    }

    public void RemoveParkingSpotAssignment()
    {
        AssignedParkingSpotId = null;
    }
}