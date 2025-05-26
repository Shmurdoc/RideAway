using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IServices
{
    public interface ILocationService
    {
        Task<double> GetDistanceAsync(Location origin, Location destination);
    }
}
