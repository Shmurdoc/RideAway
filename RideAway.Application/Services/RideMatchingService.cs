using AutoMapper;
using Microsoft.Extensions.Logging;
using RideAway.Application.DTOs;
using RideAway.Application.IRepositories;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Application.Services
{
    public class RideMatchingService : IRideMatchingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocationService _locationService;
        private readonly IGeoCodingService _geocodingService;
        private readonly IFareCalculationService _fareCalculationService;
        private readonly ILogger<RideMatchingService> _logger;

        public RideMatchingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocationService locationService,
            IGeoCodingService geocodingService,
            IFareCalculationService fareCalculationService,
            ILogger<RideMatchingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _locationService = locationService;
            _geocodingService = geocodingService;
            _fareCalculationService = fareCalculationService;
            _logger = logger;
        }

        public async Task<List<RideDTO>> FindDriverAsync(string? pickupLocation, string? destination, RideCategory rideCategory)
        {
            if (string.IsNullOrWhiteSpace(pickupLocation) || string.IsNullOrWhiteSpace(destination))
            {
                _logger.LogWarning("Pickup or destination is null or empty.");
                return new List<RideDTO>();
            }

            var drivers = await _unitOfWork.UserRepository.GetAllAsync(u =>
                u.Role == UserRole.Driver && !string.IsNullOrWhiteSpace(u.CurrentLocation));

            _logger.LogInformation("Found {Count} available drivers.", drivers.Count());

            var userLocation = await _geocodingService.ConvertAddressToLocationAsync(pickupLocation);
            var destinationLocation = await _geocodingService.ConvertAddressToLocationAsync(destination);

            if (userLocation is null || destinationLocation is null)
            {
                _logger.LogWarning("Could not geocode pickup or destination location.");
                return new List<RideDTO>();
            }

            var nearbyRides = new List<RideDTO>();

            foreach (var driver in drivers)
            {
                if (driver is null || string.IsNullOrWhiteSpace(driver.CurrentLocation))
                {
                    _logger.LogWarning("Skipping driver without a valid current location.");
                    continue;
                }

                var driverLocation = await _geocodingService.ConvertAddressToLocationAsync(driver.CurrentLocation);
                if (driverLocation is null)
                {
                    _logger.LogWarning("Could not geocode driver location. DriverId: {DriverId}", driver.Id);
                    continue;
                }

                var distance = await _locationService.GetDistanceAsync(driverLocation, userLocation);

                _logger.LogInformation("Driver {DriverId} is {Distance} km away from pickup.", driver.Id, distance);

                if (distance <= 10)
                {
                    var fare = await CalculateFareAsync(userLocation, destinationLocation, rideCategory);

                    nearbyRides.Add(new RideDTO
                    {
                        DriverId = driver.Id,
                        PickupLocation = pickupLocation,
                        Destination = destination,
                        EstimatedFare = fare
                    });
                }
            }

            _logger.LogInformation("{Count} nearby rides found within 10km radius.", nearbyRides.Count);
            return _mapper.Map<List<RideDTO>>(nearbyRides);
        }

        public async Task<decimal> CalculateFareAsync(Location pickup, Location destination, RideCategory rideCategory)
        {
            var fare = await _fareCalculationService.CalculateFareAsync(pickup, destination, rideCategory);
            _logger.LogInformation("Calculated fare: {Fare}", fare);
            return fare;
        }
    }
}
