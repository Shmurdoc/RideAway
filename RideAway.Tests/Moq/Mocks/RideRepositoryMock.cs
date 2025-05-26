using Moq;
using System.Collections.Generic;
using RideAway.Domain.Entities;
using RideAway.Application.IRepositories;
using System.Linq.Expressions;

namespace RideAway.Tests.Moq.Mocks
{
    public static class RideRepositoryMock
    {
        public static Mock<IRideRepository> GetMockIRideRepository(List<Ride> rides)
        {
            var mock = new Mock<IRideRepository>();

            mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => rides.FirstOrDefault(r => r.Id == id));

            return mock;
        }

    }
}
