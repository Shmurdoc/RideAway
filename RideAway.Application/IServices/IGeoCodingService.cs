using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IServices
{
    public interface IGeoCodingService
    {
        Task<Location> ConvertAddressToLocationAsync(string address);
    }
}
