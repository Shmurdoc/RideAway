using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IRepositories
{
    public interface IUnitOfWork
    {
        public IRideRepository RideRepository { get; }
        public IPaymentRepository PaymentRepository { get; }
        public IUserRepository UserRepository { get; }

        Task<int> SaveChangesAsync();
        void Dispose();
    }
}
