using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.Features.Rides.Queries;
using RideAway.Domain.Entities.Enum;

namespace RideAway.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result is not null ? Ok(result) : BadRequest("User not created");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id));
        return user is not null ? Ok(user) : NotFound("User not found");
    }

    [HttpGet("available-rides")]
    public async Task<IActionResult> GetAvailableRides(string? startLocation, string? endLocation, RideCategory ride)
    {
        var rides = await _mediator.Send(new GetAvailableRidesQuery(startLocation!, endLocation!, ride));
        return Ok(rides);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelRide([FromBody] CancelRideCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        var result = await _mediator.Send(command);

        return result
            ? Ok(new { Message = "Ride canceled successfully." })
            : BadRequest(new { Message = "Failed to cancel ride." });
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        var result = await _mediator.Send(command);

        return result.IsSuccessful
            ? Ok(result)
            : BadRequest(new { Message = "Payment failed.", Reason = result.FailureReason });
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestRide([FromBody] CreateRideRequestDTO? dto)
    {
        if (dto == null)
            return BadRequest("Ride request cannot be null.");

        var command = new RequestRideCommand(dto);
        var ride = await _mediator.Send(command);
        return Ok(ride);
    }
}
