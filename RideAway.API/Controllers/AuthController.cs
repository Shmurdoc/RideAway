using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideAway.Application.DTOs;
using RideAway.Application.Features.Authentication.Commands;
using RideAway.Application.IServices.IAuthentication;

namespace RideAway.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthController(IMediator mediator, IJwtTokenGenerator jwtTokenGenerator)
        {
            _mediator = mediator;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            if (command == null)
                return BadRequest(new { Message = "Invalid request. Command cannot be null." });

            var user = await _mediator.Send(command);

            var response = new LoginResponseDTO
            {
                Token = _jwtTokenGenerator.GenerateToken(user),
                UserId = user.Id,
                Name = user.Name ?? string.Empty,
                Role = user.Role.ToString()
            };

            return Ok(response);
        }
    }
}
