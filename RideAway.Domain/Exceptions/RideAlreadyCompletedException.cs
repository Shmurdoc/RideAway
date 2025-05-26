using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Exceptions
{
    public class RideAlreadyCompletedException
    {
    }

    public class RideNotFoundException : Exception
    {
        public RideNotFoundException(string message) : base(message) { }
    }

    public class InvalidRideStatusException : Exception
    {
        public InvalidRideStatusException(string message) : base(message) { }
    }

}
