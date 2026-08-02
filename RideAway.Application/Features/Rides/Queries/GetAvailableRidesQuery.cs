using MediatR;
using RideAway.Application.DTOs;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.Features.Rides.Queries
{
    public record GetAvailableRidesQuery(
        string PickupLocation,
        string Destination,
        RideCategory RideCategory
    ) : IRequest<List<RideDTO>>;
}
