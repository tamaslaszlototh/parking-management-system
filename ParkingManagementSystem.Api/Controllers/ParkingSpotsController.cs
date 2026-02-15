using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagementSystem.Application.AddParkingSpot.Commands;
using ParkingManagementSystem.Application.AssignDedicatedParkingSpot;
using ParkingManagementSystem.Application.DeactivateParkingSpot.Commands;
using ParkingManagementSystem.Application.RemoveDedicatedParkingSpotAssignment;
using ParkingManagementSystem.Contracts.ParkingSpot.AddParkingSpot;
using ParkingManagementSystem.Contracts.ParkingSpot.AssignDedicatedParkingSpot;
using ParkingManagementSystem.Contracts.ParkingSpot.DeactivateParkingSpot;

namespace ParkingManagementSystem.Api.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "ParkingAdministrator")]
public class ParkingSpotsController : ApiController
{
    private readonly IMapper _mapper;
    private readonly ISender _mediator;

    public ParkingSpotsController(IMapper mapper, ISender mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpPost("addparkingspot")]
    public async Task<IActionResult> AddParkingSpot(AddParkingSpotRequest request)
    {
        var command = _mapper.Map<AddParkingSpotCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            parkingSpot => Ok(_mapper.Map<AddParkingSpotResult>(parkingSpot)),
            error => Problem(error));
    }

    [HttpPost("assigndedicatedparkingspot")]
    public async Task<IActionResult> AssignDedicatedParkingSpot(AssignDedicatedParkingSpotRequest request)
    {
        var command = _mapper.Map<AssignDedicatedParkingSpotCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            _ => Ok(),
            error => Problem(error));
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateParkingSpot(Guid id)
    {
        var command = new DeactivateParkingSpotCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            success => Ok(_mapper.Map<DeactivateParkingSpotResponse>(success)),
            errors => Problem(errors));
    }

    [HttpPatch("{id:guid}/remove-assignment")]
    public async Task<IActionResult> RemoveDedicatedParkingSpotAssignment(Guid id)
    {
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(id);
        var result = await _mediator.Send(command);
        return result.Match(
            _ => Ok(),
            errors => Problem(errors));
    }
}