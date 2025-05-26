using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Exceptions
{
    public class PaymentProcessingException : Exception
    {
        public PaymentProcessingException(string message) : base(message) { }
    }
}
