using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IServices.INotification
{
    public interface ISmsService
    {
        void SendSms(string phoneNumber, string message);
    }
}
