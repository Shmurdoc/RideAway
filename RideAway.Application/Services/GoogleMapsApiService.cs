using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RideAway.Application.Services
{
    public class GoogleMapsApiService : IGoogleMapsApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoogleMapsApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"] ?? throw new ArgumentNullException("Google Maps API key is missing.");
        }

        public async Task<double> CalculateDistanceAsync(double originLat, double originLng, double destinationLat, double destinationLng)
        {
            string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={originLat},{originLng}&destinations={destinationLat},{destinationLng}&key={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve distance from Google Maps API.");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseBody);

            var distanceElement = jsonDoc.RootElement
                .GetProperty("rows")[0]
                .GetProperty("elements")[0]
                .GetProperty("distance")
                .GetProperty("value"); // Value is in meters

            return distanceElement.GetDouble() / 1000; // Convert meters to kilometers
        }

        public async Task<string> GetRouteAsync(Location origin, Location destination)
        {
            var originCoords = $"{origin.Coordinates.Latitude},{origin.Coordinates.Longitude}";
            var destinationCoords = $"{destination.Coordinates.Latitude},{destination.Coordinates.Longitude}";
            var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={originCoords}&destination={destinationCoords}&key={_apiKey}";

            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            var route = json["routes"]?.FirstOrDefault()?["overview_polyline"]?["points"]?.ToString();

            if (string.IsNullOrEmpty(route))
                throw new Exception("Failed to get route from Google Maps.");

            return route; 
        }
    }
}
