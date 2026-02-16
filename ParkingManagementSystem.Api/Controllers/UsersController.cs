using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagementSystem.Application.LoginUser;
using ParkingManagementSystem.Application.RegisterUser.Commands;
using ParkingManagementSystem.Contracts.User.LoginUser;
using ParkingManagementSystem.Contracts.User.RegisterUser;

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

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
    {
        var command = _mapper.Map<RegisterUserCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            user => Ok(_mapper.Map<RegisterUserResult>(user)),
            error => Problem(error));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginUser(LoginUserRequest request)
    {
        var command = _mapper.Map<LoginUserCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            success => Ok(_mapper.Map<LoginUserResponse>(success)),
            error => Problem(error));
    }
}