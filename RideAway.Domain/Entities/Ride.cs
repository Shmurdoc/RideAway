
using System.Text.Json.Serialization;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Exceptions;
using RideAway.Domain.Value_Object;

namespace RideAway.Domain.Entities
{
    public class Ride : BaseEntity
    {
        public Guid RiderId { get; set; }

        /// <summary>Never serialized; would drag the user graph into responses.</summary>
        [JsonIgnore]
        public User Rider { get; set; } = null!;

        public Guid? DriverId { get; set; }

        [JsonIgnore]
        public User? Driver { get; set; }
        public string PickupLocation { get; set; } = null!;
        public string Destination { get; set; } = null!;
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

        public void Accept(Guid driverId)
        {
            EnsureStatus(RideStatus.Requested, "accepted");
            DriverId = driverId;
            Status = RideStatus.Accepted;
        }

        public void StartTrip(Guid driverId)
        {
            EnsureStatus(RideStatus.Accepted, "started");
            EnsureAssignedDriver(driverId);
            Status = RideStatus.InProgress;
        }

        public void Complete(Guid driverId)
        {
            EnsureStatus(RideStatus.InProgress, "completed");
            EnsureAssignedDriver(driverId);
            Status = RideStatus.Completed;
        }

        public void Cancel(Guid actorId)
        {
            if (Status is RideStatus.Completed or RideStatus.Paid)
                throw new RideAlreadyCompletedException("This ride has already finished and cannot be canceled.");

            if (Status == RideStatus.Canceled)
                return;

            EnsureParticipant(actorId);
            Status = RideStatus.Canceled;
        }

        /// <summary>Settles the ride. Completed only, once. Terminal state.</summary>
        public void MarkAsPaid()
        {
            if (Status == RideStatus.Paid)
                throw new PaymentProcessingException("This ride has already been paid.");

            EnsureStatus(RideStatus.Completed, "paid");
            Status = RideStatus.Paid;
        }

        private void EnsureStatus(RideStatus expected, string action)
        {
            if (Status != expected)
                throw new InvalidRideStatusException($"A ride cannot be {action} while it is {Status}.");
        }

        private void EnsureAssignedDriver(Guid driverId)
        {
            if (DriverId != driverId)
                throw new UnauthorizedAccessException("This ride is not assigned to you.");
        }

        private void EnsureParticipant(Guid actorId)
        {
            if (RiderId != actorId && DriverId != actorId)
                throw new UnauthorizedAccessException("You are not authorized to perform this action.");
        }
    }

}
