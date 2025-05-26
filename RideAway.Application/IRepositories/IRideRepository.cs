using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IRepositories
{
    public interface IRideRepository : IGenericRepository<Ride>
    {
        Task<Ride?> UpdateAsync(Ride ride);
    }

}
