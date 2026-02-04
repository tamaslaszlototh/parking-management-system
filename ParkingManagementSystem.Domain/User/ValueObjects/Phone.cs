using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.User.ValueObjects;

public class Phone : ValueObject
{
    public string Value { get; }

    private Phone(string value)
    {
        Value = value;
    }

    public static Phone Create(string phone)
    {
        return new Phone(phone);
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}