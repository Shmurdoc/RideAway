using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Value_Object
{
    public enum PaymentStatus
    {
        Pending = 0,    // Payment is being processed
        Completed = 1,  // Payment is successful
        Failed = 2      // Payment failed
    }
}
