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
    public static class UserRepositoryMock
    {
        public static Mock<IGenericRepository<User>> GetMockUserRepository(List<User> users)
        {
            return GenericRepositoryMock.GetMockRepository(users);
        }
    }
}
