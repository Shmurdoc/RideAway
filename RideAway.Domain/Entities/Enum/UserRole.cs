using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Entities.Enum
{
    public enum UserRole
    {
        Rider,  // A user who books rides
        Driver, // A user who provides rides
        Admin   // A user with system management privileges
    }

}
