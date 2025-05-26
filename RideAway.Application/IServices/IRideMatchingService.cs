using RideAway.Application.DTOs;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IServices
{
    public interface IRideMatchingService
    {
        Task<List<RideDTO>> FindDriverAsync(string? pickupLocation, string? destination, RideCategory rideCategory);
        Task<decimal> CalculateFareAsync(Location pickup, Location destination, RideCategory rideCategory);
    }
}
