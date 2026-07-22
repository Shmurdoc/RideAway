using RideAway.Application.IRepositories;

namespace RideAway.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;

        public IRideRepository RideRepository { get; }
        public IPaymentRepository PaymentRepository { get; }
        public IUserRepository UserRepository { get; }

        public UnitOfWork(ApplicationDbContext context,
            IRideRepository rideRepository,
            IPaymentRepository paymentRepository,
            IUserRepository userRepository)
        {
            _context = context;
            RideRepository = rideRepository;
            PaymentRepository = paymentRepository;
            UserRepository = userRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
