using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkingManagementSystem.Application.ReserveParkingSpot;
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

    [HttpPost("reserveparkingspots")]
    public async Task<IActionResult> ReserveParkingSpot(ReserveParkingSpotRequest request)
    {
        var command = _mapper.Map<ReserveParkingSpotCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            _ => Ok(),
            error => Problem(error));
    }
}