using Moq;
using RideAway.Application.IRepositories;

namespace RideAway.Tests.Moq.Mocks
{
    public static class UnitOfWorkMock
    {
        public static Mock<IUnitOfWork> GetMockUnitOfWork()
        {
            var mock = new Mock<IUnitOfWork>();
            // Set up defaults or empty returns
            return mock;
        }
    }


}
