using MediatR;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities;
using Stripe.Forwarding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.Features.Rides.Commands
{
    public record CreateUserCommand(CreateUserDTO createUserDTO) : IRequest<User>;
}
