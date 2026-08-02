using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Application.Services
{
    public class FareCalculationService : IFareCalculationService
    {
        private readonly ILocationService _locationService;

        public FareCalculationService(ILocationService locationService)
        {
            _locationService = locationService;
        }

        private readonly Dictionary<RideCategory, decimal> _baseFares = new()
        {
            { RideCategory.Standard, 5.00m },
            { RideCategory.Premium, 10.00m },
            { RideCategory.Luxury, 20.00m }
        };

        private readonly Dictionary<RideCategory, decimal> _perKmRates = new()
        {
            { RideCategory.Standard, 2.00m },
            { RideCategory.Premium, 3.50m },
            { RideCategory.Luxury, 5.00m }
        };

        public async Task<decimal> CalculateFareAsync(Location pickup, Location destination, RideCategory category)
        {
            var distance = await GetDistanceAsync(pickup, destination);

            var baseFare = _baseFares[category];
            var distanceCharge = distance * _perKmRates[category];

            var surgeMultiplier = GetSurgeMultiplier();
            var totalFare = (baseFare + distanceCharge) * surgeMultiplier;

            return Math.Round(totalFare, 2);
        }

        private async Task<decimal> GetDistanceAsync(Location pickup, Location destination)
        {
            var distanceInKm = await _locationService.GetDistanceAsync(pickup, destination);
            return Math.Max((decimal)distanceInKm, 0.5m);
        }

        private decimal GetSurgeMultiplier()
        {
            var hour = DateTime.Now.Hour;
            return (hour >= 7 && hour <= 9) || (hour >= 17 && hour <= 20) ? 1.5m : 1.0m;
        }
    }
}
