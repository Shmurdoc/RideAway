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

    /// <summary>
    /// Creates a new user (rider or driver). Does not require authentication.
    /// </summary>
    /// <param name="command">The user creation payload.</param>
    /// <returns>The created user, or 400 if the user could not be created.</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result is not null ? Ok(result) : BadRequest("User not created");
    }

    /// <summary>
    /// Gets a user by id.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <returns>The user, or 404 if no user matches the id.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id));
        return user is not null ? Ok(user) : NotFound("User not found");
    }

    /// <summary>
    /// Finds available rides by matching nearby drivers within the configured radius.
    /// </summary>
    /// <param name="startLocation">The pickup address.</param>
    /// <param name="endLocation">The destination address.</param>
    /// <param name="ride">The requested ride category.</param>
    /// <returns>A list of ride offers with estimated fares.</returns>
    [HttpGet("available-rides")]
    public async Task<IActionResult> GetAvailableRides(string? startLocation, string? endLocation, RideCategory ride)
    {
        var rides = await _mediator.Send(new GetAvailableRidesQuery(startLocation!, endLocation!, ride));
        return Ok(rides);
    }

    /// <summary>
    /// Cancels a ride requested by the current user.
    /// </summary>
    /// <param name="command">The ride id to cancel.</param>
    /// <returns>200 if the ride was canceled, 400 otherwise.</returns>
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

    /// <summary>
    /// Processes a payment for a ride.
    /// </summary>
    /// <param name="command">The payment details.</param>
    /// <returns>The payment result, or 400 if the payment failed.</returns>
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

    /// <summary>
    /// Requests a new ride.
    /// </summary>
    /// <param name="dto">The ride request payload.</param>
    /// <returns>The created ride, or 400 if the request is invalid.</returns>
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
