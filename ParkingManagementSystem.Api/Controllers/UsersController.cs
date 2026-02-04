using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkingManagementSystem.Application.RegisterUser.Commands;
using ParkingManagementSystem.Contracts.User;

namespace ParkingManagementSystem.Api.Controllers;

[Route("api/[controller]")]
public class UsersController : ApiController
{
    private readonly IMapper _mapper;
    private readonly ISender _mediator;

    public UsersController(IMapper mapper, ISender mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpPost] //Todo: Authorize for Parking Administrator only
    public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
    {
        var command = _mapper.Map<RegisterUserCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            user => Ok(_mapper.Map<RegisterUserResult>(user)),
            error => Problem(error));
    }
}