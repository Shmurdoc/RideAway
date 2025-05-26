using Moq;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Tests.Moq.Mocks
{
    public static class PaymentRepositoryMock
    {
        public static Mock<IPaymentRepository> GetMockIPaymentRepository(List<Payment> payments)
        {
            var mock = new Mock<IPaymentRepository>();

            mock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
                .Callback<Payment>(payment => payments.Add(payment))
                .Returns(Task.CompletedTask);

            // Add more setups if needed (GetByIdAsync, etc.)

            return mock;
        }
    }
}
