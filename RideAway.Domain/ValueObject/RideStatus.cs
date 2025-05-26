using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Value_Object
{
    public enum RideStatus
    {
        Requested = 0,   // Rider requested a ride
        Accepted = 1,    // Driver accepted the ride
        InProgress = 2,  // Ride is ongoing
        Completed = 3,   // Ride is finished
        Canceled = 4     // Ride was canceled
    }

}
