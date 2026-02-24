using Mapster;
using ParkingManagementSystem.Application.LoginUser;
using ParkingManagementSystem.Application.LoginUser.Models;
using ParkingManagementSystem.Application.RegisterUser.Commands;
using ParkingManagementSystem.Contracts.User;
using ParkingManagementSystem.Contracts.User.LoginUser;
using ParkingManagementSystem.Contracts.User.RegisterUser;
using ParkingManagementSystem.Domain.User;
using UserRole = ParkingManagementSystem.Domain.User.Enums.UserRole;

namespace ParkingManagementSystem.Api.Mappings;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterUserRequest, RegisterUserCommand>()
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Password, src => src.Password)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Role, src => src.Role);

        config.NewConfig<User, RegisterUserResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FirstName, src => src.FirstName.Value)
            .Map(dest => dest.LastName, src => src.LastName.Value)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.Phone, src => src.Phone.Value)
            .Map(dest => dest.Role, src => src.Role);
        
        config.NewConfig<LoginUserRequest, LoginUserCommand>()
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Password, src => src.Password);

        config.NewConfig<LoginUserResult, LoginUserResponse>()
            .Map(dest => dest.Token, src => src.Token)
            .Map(dest => dest.UserId, src => src.User.Id)
            .Map(dest => dest.FirstName, src => src.User.FirstName.Value)
            .Map(dest => dest.LastName, src => src.User.LastName.Value)
            .Map(dest => dest.Email, src => src.User.Email)
            .Map(dest => dest.Roles, src => new List<UserRole> { src.User.Role });
    }
}