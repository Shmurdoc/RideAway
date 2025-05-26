using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;

namespace RideAway.Infrastructure.Persistence.Repositories
{
    public class RideRepository : GenericRepository<Ride>, IRideRepository
    {
        private readonly ApplicationDbContext _context;

        public RideRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<Ride?> UpdateAsync(Ride ride)
        {
            _context.Rides.Update(ride);
            return Task.FromResult(ride);
        }

    }
}
