using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using System;
using System.Threading.Tasks;

namespace RideAway.Application.Services
{
    public class FareCalculationService : IFareCalculationService
    {
        private readonly ILocationService _locationService;

        public FareCalculationService(ILocationService locationService)
        {
            _locationService = locationService;
        }

        // This class serves as a simulation with method definitions present but lacking actual implementations.



        // Simulated base fares for different ride categories
        private readonly Dictionary<RideCategory, decimal> _baseFares = new()
    {
        { RideCategory.Standard, 5.00m },
        { RideCategory.Premium, 10.00m },
        { RideCategory.Luxury, 20.00m }
    };

        // Per kilometer charge for each ride category
        private readonly Dictionary<RideCategory, decimal> _perKmRates = new()
    {
        { RideCategory.Standard, 2.00m },
        { RideCategory.Premium, 3.50m },
        { RideCategory.Luxury, 5.00m }
    };

        public async Task<decimal> CalculateFareAsync(Location pickup, Location destination, RideCategory category)
        {
            // Simulated distance calculation (In reality, integrate Google Maps API Below)
            var distance = await GetDistanceAsync(pickup, destination);
            // Integrated with Google Maps API
            // var distance = await _locationService.GetDistanceAsync(pickup, destination);


            // Calculate the fare
            var baseFare = _baseFares[category];
            var distanceCharge = distance * _perKmRates[category];

            // Apply surge pricing if necessary
            var surgeMultiplier = GetSurgeMultiplier();
            var totalFare = (baseFare + distanceCharge) * surgeMultiplier;

            return Math.Round(totalFare, 2);
        }

        // For logic reasons, this method is not implemented in the interface.
        // It is not necessary for the current application, but it is a good practice to include it in the interface.
        // Must hide or delete after, beacuse a method similer to this with the exact function exists in the LocationService.cs file.
        // For only Simulaton purpose only 
        private async Task<decimal> GetDistanceAsync(Location pickup, Location destination)
        {
            // Simulated distance calculation (Returns a random distance between 3-15 km)
            await Task.Delay(100); // Simulating API call delay
            Random random = new();
            return random.Next(3, 15);
        }

        private decimal GetSurgeMultiplier()
        {
            // Simulate surge pricing based on random peak times
            var hour = DateTime.Now.Hour;
            return (hour >= 7 && hour <= 9) || (hour >= 17 && hour <= 20) ? 1.5m : 1.0m;
        }
    }
}
