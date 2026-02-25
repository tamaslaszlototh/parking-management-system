using Mapster;
using ParkingManagementSystem.Application.CancelReservation;
using ParkingManagementSystem.Application.GetReservationsForDates.Commands;
using ParkingManagementSystem.Application.GetReservationsForDates.Models;
using ParkingManagementSystem.Application.GetReservationsForUser.Commands;
using ParkingManagementSystem.Application.GetReservationsForUser.Models;
using ParkingManagementSystem.Application.ReserveParkingSpot;
using ParkingManagementSystem.Contracts.Reservation.CancelReservation;
using ParkingManagementSystem.Contracts.Reservation.GetReservations;
using ParkingManagementSystem.Contracts.Reservation.ReserveParkingSpot;
using ParkingManagementSystem.Domain.Reservation;

namespace ParkingManagementSystem.Api.Mappings;

public class ReservationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ReserveParkingSpotRequest, ReserveParkingSpotCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Dates, src => src.Dates)
            .Map(dest => dest.PreferDedicatedParkingSpots, src => src.PreferDedicatedParkingSpots);

        config.NewConfig<CancelReservationRequest, CancelReservationCommand>()
            .Map(dest => dest.ReservationIds, src => src.ReservationIds)
            .Map(dest => dest.UserId, src => src.UserId);

        config.NewConfig<List<DateOnly>, CancelReservationResponse>()
            .Map(dest => dest.CancelledDates, src => src);

        config.NewConfig<GetReservationsForDatesRequest, GetReservationsForDatesCommand>()
            .Map(dest => dest.Dates, src => src.Dates);

        config.NewConfig<Reservation, ReservationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ParkingSpotId, src => src.ParkingSpotId)
            .Map(dest => dest.ReservationDate, src => src.ReservationDate);

        config.NewConfig<GetReservationsForDatesResult, GetReservationsForDatesResponse>()
            .Map(dest => dest.Reservations, src => src.Reservations);

        config.NewConfig<GetReservationsForUserRequest, GetReservationsForUserCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.IncludeExpired, src => src.IncludeExpired);

        config.NewConfig<GetReservationsForUserResult, GetReservationsForUserResponse>()
            .Map(dest => dest.Reservations, src => src.Reservations);
    }
}