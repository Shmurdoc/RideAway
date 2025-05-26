using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Service
{
    public class RideFactory : IRideFactory
    {
        public Ride CreateRide(string pickupLocation, string destination, decimal fare, Guid driverId)
        {
            return new Ride(pickupLocation, destination, fare)
            {
                DriverId = driverId,
                Status = RideStatus.Requested
            };
        }
    }

}
