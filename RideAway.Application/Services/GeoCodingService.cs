using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RideAway.Application.IServices;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;

namespace RideAway.Application.Services
{
    public class GoogleGeocodingService : IGeoCodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoogleGeocodingService> _logger;

        public GoogleGeocodingService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleGeocodingService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["GoogleMaps:ApiKey"] ?? throw new ArgumentNullException("Google Maps API key is missing.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Location?> ConvertAddressToLocationAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));

            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";

            try
            {
                _logger.LogInformation("Sending geocode request for address: {Address}", address);

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var results = json["results"] as JArray;
                if (results is null || results.Count == 0)
                {
                    _logger.LogWarning("No geocoding results for address: {Address}", address);
                    return null;
                }

                var locationToken = results[0]?["geometry"]?["location"];
                if (locationToken is null)
                {
                    _logger.LogWarning("No location data in geocoding response for address: {Address}", address);
                    return null;
                }

                var latitude = locationToken["lat"]?.Value<double>() ?? 0;
                var longitude = locationToken["lng"]?.Value<double>() ?? 0;

                _logger.LogInformation("Geocoded {Address} to lat: {Lat}, lng: {Lng}", address, latitude, longitude);

                return new Location(new GeoLocation(latitude, longitude), address);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error connecting to Google Geocoding API for address: {Address}", address);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during geocoding for address: {Address}", address);
                throw;
            }
        }
    }
}

