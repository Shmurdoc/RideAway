using RideAway.Application.DTOs;
using RideAway.Domain.Entities;
using RideAway.Tests.Moq.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Tests.Moq.DTO
{
    public static class RideTestFixture
    {
        public static CreateRideRequestDTO ValidRideDTO() => RideFactory.GenerateRideRequestDTO();
        public static Ride ValidRide() => RideFactory.GenerateRides(1).First();
    }

}
