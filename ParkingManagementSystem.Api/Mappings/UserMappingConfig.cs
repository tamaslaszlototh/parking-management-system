using Mapster;
using ParkingManagementSystem.Application.RegisterUser.Commands;
using ParkingManagementSystem.Contracts.User;
using ParkingManagementSystem.Domain.User;

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
    }
}