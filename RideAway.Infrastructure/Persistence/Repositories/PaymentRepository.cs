using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;

namespace RideAway.Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<Payment?> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment!);
            return Task.FromResult(payment);
        }

    }
}
