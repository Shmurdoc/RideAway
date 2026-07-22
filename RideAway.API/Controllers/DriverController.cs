using MediatR;
using Microsoft.AspNetCore.Mvc;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Rides.Commands;

namespace RideAway.API.Controllers;

[ApiController]
[Route("api/drivers")]
public class DriverController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriverController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("update-location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateDriverLocationCommand command)
    {
        var result = await _mediator.Send(command);

        return result
            ? Ok(new { Success = true, Message = "Location updated." })
            : BadRequest(new { Success = false, Message = "Failed to update location." });
    }

    [HttpPost("collect-rider")]
    public async Task<IActionResult> CollectRider([FromBody] CollectRiderCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        var ride = await _mediator.Send(command);

        if (ride == null)
            return NotFound(new { Message = "Ride not found or driver mismatch." });

        return Ok(new { Message = "Rider collected. Ride in progress.", Data = ride });
    }

    [HttpPost("process-payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccessful ? Ok(result) : BadRequest(result);
    }

    [HttpPost("accept")]
    public async Task<IActionResult> AcceptRide([FromBody] AcceptRideCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        var result = await _mediator.Send(command);

        return result
            ? Ok(new { Message = "Ride accepted successfully." })
            : BadRequest(new { Message = "Failed to accept the ride." });
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
}
