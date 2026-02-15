using Mapster;
using ParkingManagementSystem.Application.AddParkingSpot.Commands;
using ParkingManagementSystem.Application.AssignDedicatedParkingSpot;
using ParkingManagementSystem.Application.DeactivateParkingSpot.Models;
using ParkingManagementSystem.Contracts.ParkingSpot.AddParkingSpot;
using ParkingManagementSystem.Contracts.ParkingSpot.AssignDedicatedParkingSpot;
using ParkingManagementSystem.Contracts.ParkingSpot.DeactivateParkingSpot;
using ParkingManagementSystem.Domain.ParkingSpot;

namespace ParkingManagementSystem.Api.Mappings;

public class ParkingSpotMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AddParkingSpotRequest, AddParkingSpotCommand>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.State, src => src.State);

        config.NewConfig<ParkingSpot, AddParkingSpotResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Description, src => src.Description.Value)
            .Map(dest => dest.State, src => src.State);

        config.NewConfig<AssignDedicatedParkingSpotRequest, AssignDedicatedParkingSpotCommand>()
            .Map(dest => dest.ManagerId, src => src.ManagerId)
            .Map(dest => dest.ParkingSpotId, src => src.ParkingSpotId);

        config.NewConfig<DeactivateParkingSpotCommandResult, DeactivateParkingSpotResponse>()
            .Map(dest => dest.ReservationIds, src => src.ReservationIds)
            .Map(dest => dest.LastReservedDate, src => src.LastReservedDate);
    }
}