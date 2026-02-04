using System.Text.RegularExpressions;
using ParkingManagementSystem.Domain.Common;

namespace ParkingManagementSystem.Domain.User.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    private const int MaxLength = 255;

    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var trimmedEmail = email.Trim().ToLowerInvariant();

        if (trimmedEmail.Length > MaxLength)
            throw new ArgumentException($"Email cannot exceed {MaxLength} characters", nameof(email));

        if (!EmailRegex.IsMatch(trimmedEmail))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(trimmedEmail);
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}