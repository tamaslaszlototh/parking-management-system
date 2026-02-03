using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.AddParkingSpot.ValueObjects;

public sealed class ParkingSpotName : ValueObject
{
    public string Value { get; private set; }

    private ParkingSpotName(string value)
    {
        Value = value;
    }

    public static ParkingSpotName Create(string name)
    {
        return new ParkingSpotName(name);
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}