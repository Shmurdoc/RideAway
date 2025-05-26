
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;

namespace RideAway.Domain.Entities
{
    public class Ride : BaseEntity
    {
        public Guid RiderId { get; set; }
        public User Rider { get; set; } = null!;
        public Guid DriverId { get; set; }
        public User? Driver { get; set; }
        public string? PickupLocation { get; set; } = null!;
        public string? Destination { get; set; } = null!;
        public decimal Fare { get; set; }
        public RideCategory RiderCategory { get; set; }
        public RideStatus Status { get; set; } = RideStatus.Requested;

        // Parameterless constructor for EF Core and object initialization
        public Ride() { }

        // Additional constructor for specific scenarios
        public Ride(string pickup, string destination, decimal fare)
        {
            if (fare < 0)
               throw new ArgumentException("Fare must be a non-negative value.", nameof(fare));

            PickupLocation = pickup ?? throw new ArgumentNullException(nameof(pickup));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
            Fare = fare;
            Status = RideStatus.Requested;
        }

        public void MarkAsPaid()
        {
            this.Status = RideStatus.Completed;
        }
    }

}
