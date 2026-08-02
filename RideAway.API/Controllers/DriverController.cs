using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Rides.Commands;

namespace RideAway.API.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize(Roles = "Driver")]
public class DriverController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriverController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Updates the current location of the authenticated driver.
    /// </summary>
    /// <param name="command">The driver's new location.</param>
    /// <returns>200 if the location was updated, 400 otherwise.</returns>
    [HttpPost("update-location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateDriverLocationCommand command)
    {
        var result = await _mediator.Send(command);

        return result
            ? Ok(new { Success = true, Message = "Location updated." })
            : BadRequest(new { Success = false, Message = "Failed to update location." });
    }

    /// <summary>
    /// Starts a ride after collecting the rider.
    /// </summary>
    /// <param name="command">The ride and current location details.</param>
    /// <returns>The in-progress ride, or 404 if the ride does not belong to the driver.</returns>
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

    /// <summary>
    /// Processes a payment for a completed ride.
    /// </summary>
    /// <param name="command">The payment details.</param>
    /// <returns>The payment result, or 400 if the payment failed.</returns>
    [HttpPost("process-payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccessful ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Accepts a ride assigned to the authenticated driver.
    /// </summary>
    /// <param name="command">The ride id to accept.</param>
    /// <returns>200 if the ride was accepted, 400 otherwise.</returns>
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

    /// <summary>
    /// Cancels a ride assigned to the authenticated driver.
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
}
