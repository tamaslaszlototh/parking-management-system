namespace ParkingManagementSystem.Application.Common.Services;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}