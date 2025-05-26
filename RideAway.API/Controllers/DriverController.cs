using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RideAway.Application.Features.Payments.Commands;
using RideAway.Application.Features.Ride.Commands;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Domain.Exceptions;


namespace RideAway.API.Controllers;

[ApiController]
[Route("api/drivers")]
public class DriverController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DriverController> _logger;

    public DriverController(IMediator mediator, ILogger<DriverController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    // Update Driver Location 
    [HttpPost("update-location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateDriverLocationCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (result)
            {
                return Ok(new { Success = true, Message = "Location updated." });
            }

            return BadRequest(new { Success = false, Message = "Failed to update location." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
        }
    }


    // Collect Rider
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
        {
            _logger.LogWarning("Invalid AcceptRide request.");
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });
        }

        try
        {
            var result = await _mediator.Send(command);

            if (result)
            {
                return Ok(new { Message = "Ride accepted successfully." });
            }

            return BadRequest(new { Message = "Failed to accept the ride." });
        }
        catch (RideNotFoundException)
        {
            return NotFound(new { Message = "Ride not found." });
        }
        catch (InvalidRideStatusException)
        {
            return BadRequest(new { Message = "Ride has already been accepted or is not available." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error accepting ride.");
            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }
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

}
