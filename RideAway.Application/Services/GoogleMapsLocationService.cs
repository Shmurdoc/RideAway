using RideAway.Application.IServices;
using RideAway.Domain.Value_Object;
using GoogleMapsApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RideAway.Domain.Entities;

namespace RideAway.Application.Services
{
    public class GoogleMapsLocationService : ILocationService
    {
        private readonly IGoogleMapsApi _googleMapsApi;

        public GoogleMapsLocationService(IGoogleMapsApi googleMapsApi)
        {
            _googleMapsApi = googleMapsApi;
         }
        public async Task<double> GetDistanceAsync(Location origin, Location destination)
        {
            return await _googleMapsApi.CalculateDistanceAsync(origin.Coordinates.Latitude, origin.Coordinates.Longitude, destination.Coordinates.Latitude, destination.Coordinates.Longitude);
        }
    }

    
}
