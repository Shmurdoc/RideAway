using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideAway.API.Extensions;
using RideAway.Application.Features.Rides.Commands;
using RideAway.Application.IServices;

namespace RideAway.API.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize(Roles = "Driver")]
public class DriverController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentProcessingService _paymentProcessingService;

    public DriverController(IMediator mediator, IPaymentProcessingService paymentProcessingService)
    {
        _mediator = mediator;
        _paymentProcessingService = paymentProcessingService;
    }

    /// <summary>Updates the authenticated driver's location.</summary>
    /// <param name="dto">The new location.</param>
    [HttpPost("update-location")]
    public async Task<IActionResult> UpdateLocation([FromBody] DriverLocationUpdateRequest dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.CurrentLocation))
            return BadRequest(new { Message = "A location is required." });

        var command = new UpdateDriverLocationCommand(
            new RideAway.Application.DTOs.DriverLocationUpdateDTO { CurrentLocation = dto.CurrentLocation },
            User.GetUserId());

        var result = await _mediator.Send(command);

        return result
            ? Ok(new { Success = true, Message = "Location updated." })
            : BadRequest(new { Success = false, Message = "Failed to update location." });
    }

    /// <summary>Starts a ride after collecting the rider.</summary>
    /// <param name="command">The ride to start.</param>
    [HttpPost("collect-rider")]
    public async Task<IActionResult> CollectRider([FromBody] CollectRiderCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        var ride = await _mediator.Send(command with { DriverId = User.GetUserId() });

        if (ride == null)
            return NotFound(new { Message = "Ride not found." });

        return Ok(new { Message = "Rider collected. Ride in progress.", Data = ride });
    }

    /// <summary>Completes an in-progress ride.</summary>
    /// <param name="command">The ride to complete.</param>
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteRide([FromBody] CompleteRideCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        var result = await _mediator.Send(command with { DriverId = User.GetUserId() });

        return result
            ? Ok(new { Message = "Ride completed successfully." })
            : BadRequest(new { Message = "Failed to complete ride." });
    }

    /// <summary>Confirms cash collected for a ride. Settles the payment.</summary>
    /// <param name="command">The ride the cash was collected for.</param>
    [HttpPost("confirm-cash")]
    public async Task<IActionResult> ConfirmCash([FromBody] ConfirmCashCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        await _paymentProcessingService.ConfirmCashCollectionAsync(command.RideId, User.GetUserId());

        return Ok(new { Message = "Cash collection confirmed. Ride marked as paid." });
    }

    /// <summary>Accepts a ride.</summary>
    /// <param name="command">The ride id.</param>
    [HttpPost("accept")]
    public async Task<IActionResult> AcceptRide([FromBody] AcceptRideCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        var result = await _mediator.Send(command with { DriverId = User.GetUserId() });

        return result
            ? Ok(new { Message = "Ride accepted successfully." })
            : BadRequest(new { Message = "Failed to accept the ride." });
    }

    /// <summary>Cancels a ride the driver is party to.</summary>
    /// <param name="command">The ride id.</param>
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelRide([FromBody] CancelRideCommand command)
    {
        if (command == null)
            return BadRequest(new { Message = "Invalid request. Command cannot be null." });

        var result = await _mediator.Send(command with { RequesterId = User.GetUserId() });

        return result
            ? Ok(new { Message = "Ride canceled successfully." })
            : BadRequest(new { Message = "Failed to cancel ride." });
    }
}

/// <summary>Body for a driver location report. The driver is taken from the token.</summary>
public class DriverLocationUpdateRequest
{
    public string CurrentLocation { get; set; } = string.Empty;
}

/// <summary>Body for confirming cash collection.</summary>
public class ConfirmCashCommand
{
    public Guid RideId { get; set; }
}
