using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IRepositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
       Task<Payment?> UpdateAsync(Payment payment);
    }
}
