using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkingManagementSystem.Application.CancelReservation;
using ParkingManagementSystem.Application.GetReservationsForDates.Commands;
using ParkingManagementSystem.Application.ReserveParkingSpot;
using ParkingManagementSystem.Contracts.Reservation.CancelReservation;
using ParkingManagementSystem.Contracts.Reservation.GetReservations;
using ParkingManagementSystem.Contracts.Reservation.ReserveParkingSpot;

namespace ParkingManagementSystem.Api.Controllers;

[Route("api/[controller]")]
public class ReservationsController : ApiController
{
    private readonly IMapper _mapper;
    private readonly ISender _mediator;

    public ReservationsController(IMapper mapper, ISender mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpPost("reserve-parking-spots")]
    public async Task<IActionResult> ReserveParkingSpot(ReserveParkingSpotRequest request)
    {
        var command = _mapper.Map<ReserveParkingSpotCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            _ => Ok(),
            error => Problem(error));
    }

    [HttpPost("cancel-reservation")]
    public async Task<IActionResult> CancelReservation(CancelReservationRequest request)
    {
        var command = _mapper.Map<CancelReservationCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            success => Ok(_mapper.Map<CancelReservationResponse>(success)),
            error => Problem(error));
    }

    [HttpGet]
    public async Task<IActionResult> GetReservationsForDates([FromQuery] GetReservationsForDatesRequest request)
    {
        var command = _mapper.Map<GetReservationsForDatesCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            success => Ok(_mapper.Map<GetReservationsForDatesResponse>(success)),
            error => Problem(error));
    }
}