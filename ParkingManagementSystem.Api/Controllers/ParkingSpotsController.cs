using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagementSystem.Application.AddParkingSpot.Commands;
using ParkingManagementSystem.Application.AssignDedicatedParkingSpot;
using ParkingManagementSystem.Application.DeactivateParkingSpot.Commands;
using ParkingManagementSystem.Application.GetParkingSpots.Commands;
using ParkingManagementSystem.Application.RemoveDedicatedParkingSpotAssignment;
using ParkingManagementSystem.Contracts.ParkingSpot.AddParkingSpot;
using ParkingManagementSystem.Contracts.ParkingSpot.AssignDedicatedParkingSpot;
using ParkingManagementSystem.Contracts.ParkingSpot.DeactivateParkingSpot;
using ParkingManagementSystem.Contracts.ParkingSpot.GetParkingSpots;

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

    [HttpPost("add-parking-spot")]
    [Authorize(Roles = "ParkingAdministrator")]
    public async Task<IActionResult> AddParkingSpot(AddParkingSpotRequest request)
    {
        var command = _mapper.Map<AddParkingSpotCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            parkingSpot => Ok(_mapper.Map<AddParkingSpotResult>(parkingSpot)),
            error => Problem(error));
    }

    [HttpPost("assign-dedicated-parking-spot")]
    [Authorize(Roles = "ParkingAdministrator")]
    public async Task<IActionResult> AssignDedicatedParkingSpot(AssignDedicatedParkingSpotRequest request)
    {
        var command = _mapper.Map<AssignDedicatedParkingSpotCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            _ => Ok(),
            error => Problem(error));
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "ParkingAdministrator")]
    public async Task<IActionResult> DeactivateParkingSpot(Guid id)
    {
        var command = new DeactivateParkingSpotCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            success => Ok(_mapper.Map<DeactivateParkingSpotResponse>(success)),
            errors => Problem(errors));
    }

    [HttpPatch("{id:guid}/remove-assignment")]
    [Authorize(Roles = "ParkingAdministrator")]
    public async Task<IActionResult> RemoveDedicatedParkingSpotAssignment(Guid id)
    {
        var command = new RemoveDedicatedParkingSpotAssignmentCommand(id);
        var result = await _mediator.Send(command);
        return result.Match(
            _ => Ok(),
            errors => Problem(errors));
    }

    [HttpGet]
    [Authorize(Roles = "ParkingAdministrator,BusinessManager,Employee")]
    public async Task<IActionResult> GetParkingSpots()
    {
        var command = new GetParkingSpotsCommand();
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(_mapper.Map<GetParkingSpotsResponse>(value)),
            errors => Problem(errors));
    }
}