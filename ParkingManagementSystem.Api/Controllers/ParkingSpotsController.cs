using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkingManagementSystem.Application.AddParkingSpot.Commands;
using ParkingManagementSystem.Contracts.ParkingSpot.AddParkingSpot;

namespace ParkingManagementSystem.Api.Controllers;

[Route("api/[controller]")]
public class ParkingSpotsController : ApiController
{
    private readonly IMapper _mapper;
    private readonly ISender _mediator;

    public ParkingSpotsController(IMapper mapper, ISender mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpPost] //Todo: Authorize for Parking Administrator only
    public async Task<IActionResult> AddParkingSpot(AddParkingSpotRequest request)
    {
        var command = _mapper.Map<AddParkingSpotCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            parkingSpot => Ok(_mapper.Map<AddParkingSpotResult>(parkingSpot)),
            error => Problem(error));
    }
}