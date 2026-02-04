using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.ParkingSpot.ValueObjects;

public sealed class ParkingSpotDescription : ValueObject
{
    public string Value { get; }

    private ParkingSpotDescription(string value)
    {
        Value = value;
    }

    public static ParkingSpotDescription Create(string description)
    {
        return new ParkingSpotDescription(description);
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}