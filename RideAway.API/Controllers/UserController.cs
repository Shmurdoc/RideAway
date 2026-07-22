using MediatR;
using Microsoft.AspNetCore.Mvc;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.Features.Rides.Queries;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Exceptions;

namespace RideAway.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    public UserController(IMediator mediator, ILogger<UserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
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
        var rides = await _mediator.Send(new GetAvailableRidesQuery(endLocation!, startLocation!, ride));
        return Ok(rides);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelRide([FromBody] CancelRideCommand command)
    {
        if (command == null)
        {
            _logger.LogWarning("Invalid CancelRide request.");
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });
        }

        try
        {
            var result = await _mediator.Send(command);

            return result
                ? Ok(new { Message = "Ride canceled successfully." })
                : BadRequest(new { Message = "Failed to cancel ride." });
        }
        catch (RideNotFoundException)
        {
            return NotFound(new { Message = "Ride not found." });
        }
        catch (InvalidRideStatusException)
        {
            return BadRequest(new { Message = "Ride has already been completed and cannot be canceled." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error canceling ride.");
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        if (command == null)
        {
            _logger.LogWarning("Invalid ProcessPayment request.");
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });
        }

        try
        {
            var result = await _mediator.Send(command);

            return result.IsSuccessful
                ? Ok(result)
                : BadRequest(new { Message = "Payment failed.", Reason = result.FailureReason });
        }
        catch (RideNotFoundException)
        {
            return NotFound(new { Message = "Ride not found." });
        }
        catch (PaymentProcessingException)
        {
            return StatusCode(500, new { Message = "Payment processing encountered an error." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment.");
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestRide([FromBody] CreateRideRequestDTO dto)
    {
        if (dto == null)
            return BadRequest("Ride request cannot be null.");

        var command = new RequestRideCommand(dto);

        try
        {
            var ride = await _mediator.Send(command);
            return Ok(ride);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
