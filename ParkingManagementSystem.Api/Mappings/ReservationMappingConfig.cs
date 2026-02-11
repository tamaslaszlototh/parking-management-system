using Mapster;
using ParkingManagementSystem.Application.CancelReservation;
using ParkingManagementSystem.Application.ReserveParkingSpot;
using ParkingManagementSystem.Contracts.Reservation.CancelReservation;
using ParkingManagementSystem.Contracts.Reservation.ReserveParkingSpot;

namespace ParkingManagementSystem.Api.Mappings;

public class ReservationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ReserveParkingSpotRequest, ReserveParkingSpotCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Dates, src => src.Dates);

        config.NewConfig<CancelReservationRequest, CancelReservationCommand>()
            .Map(dest => dest.ReservationIds, src => src.ReservationIds)
            .Map(dest => dest.UserId, src => src.UserId);

        config.NewConfig<List<DateOnly>, CancelReservationResponse>()
            .Map(dest => dest.CancelledDates, src => src);
    }
}