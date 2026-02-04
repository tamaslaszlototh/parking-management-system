using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.User.ValueObjects;

public class UserName : ValueObject
{
    public string Value { get; }
    
    private const int MinLength = 2;
    private const int MaxLength = 20;

    private UserName(string value)
    {
        Value = value;
    }

    public static UserName Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length < MinLength || name.Length > MaxLength)
            throw new ArgumentException($"Name must be between {MinLength} and {MaxLength} characters long");

        return new UserName(name);
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}