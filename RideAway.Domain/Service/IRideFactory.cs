using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Service
{
    public interface IRideFactory
    {
        Ride CreateRide(string pickupLocation, string destination, decimal fare, Guid driverId);
    }

}
