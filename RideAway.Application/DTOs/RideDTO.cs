using EntityRideStatus = RideAway.Domain.Value_Object.RideStatus;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Entities;

namespace RideAway.Application.DTOs
{

    public class CreateRideRequestDTO
    {
        public Guid DriverId { get; set; }
        public string PickupLocation { get; set; } = null!;
        public string Destination { get; set; } = null!;
        public decimal EstimatedFare { get; set; }
        public RideCategory RideCategory { get; set; }
    }

    public class RideDTO : CreateRideRequestDTO
    {
        public Guid RiderId { get; set; }
        public EntityRideStatus Status { get; set; }
    }

}
