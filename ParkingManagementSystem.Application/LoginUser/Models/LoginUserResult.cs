using ParkingManagementSystem.Domain.User;

namespace ParkingManagementSystem.Application.LoginUser.Models;

public record LoginUserResult(string Token, User User);