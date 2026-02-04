using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.User.ValueObjects;

public class Password : ValueObject
{
    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Password Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return new Password(password);
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}