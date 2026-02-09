using Mapster;
using ParkingManagementSystem.Application.ReserveParkingSpot;
using ParkingManagementSystem.Contracts.Reservation.ReserveParkingSpot;

namespace ParkingManagementSystem.Api.Mappings;

public class ReservationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ReserveParkingSpotRequest, ReserveParkingSpotCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ParkingSpotId, src => src.ParkingSpotId)
            .Map(dest => dest.Date, src => src.Date);
    }
}