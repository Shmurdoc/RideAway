using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;

namespace RideAway.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _Context;
        public UserRepository(ApplicationDbContext Context) : base(Context)
        {
            _Context = Context;
        }

       
    }
}
