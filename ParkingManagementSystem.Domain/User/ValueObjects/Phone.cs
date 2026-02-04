using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.User.ValueObjects;

public class Phone : ValueObject
{
    public string Value { get; }

    private const int MaxLength = 50;

    private Phone(string value)
    {
        Value = value;
    }

    public static Phone Create(string phone)
    {
        if (phone.Length > MaxLength)
        {
            throw new ArgumentException($"Phone number cannot be longer than {MaxLength} characters");
        }

        return new Phone(phone);
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}