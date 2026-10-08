using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Value_Object
{
    public class GeoLocation
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        private GeoLocation() { } // Required for EF Core

        public GeoLocation(double latitude, double longitude)
        {
            // NaN fails every comparison, so a plain range check lets it through.
            if (!double.IsFinite(latitude) || latitude < -90 || latitude > 90)
                throw new ArgumentException("Latitude must be a finite value between -90 and 90 degrees.", nameof(latitude));
            if (!double.IsFinite(longitude) || longitude < -180 || longitude > 180)
                throw new ArgumentException("Longitude must be a finite value between -180 and 180 degrees.", nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }

        public override bool Equals(object? obj)
        {
            if (obj is GeoLocation other)
            {
                return Latitude == other.Latitude && Longitude == other.Longitude;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude);
        }
    }

}
