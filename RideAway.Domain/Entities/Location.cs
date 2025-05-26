using RideAway.Domain.Value_Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Entities
{
    public class Location
    {
        public GeoLocation? Coordinates { get; set; }
        public string Address { get; set; } = string.Empty;


        private Location() { } // Required for EF Core

        public Location(double latitude, double longitude, string address)
        {
            Coordinates = new GeoLocation(latitude, longitude);
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public Location(GeoLocation coordinates, string address)
        {
            Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }
    }
}
