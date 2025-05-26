using Moq;
using RideAway.Application.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RideAway.Tests.Moq.Mocks
{

    public static class GenericRepositoryMock
    {
        public static Mock<IGenericRepository<T>> GetMockRepository<T>(List<T> items) where T : class
        {
            var mockRepo = new Mock<IGenericRepository<T>>();

            mockRepo.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<string?>()))
                    .ReturnsAsync((Expression<Func<T, bool>> predicate, string? include) =>
                        items.AsQueryable().Where(predicate.Compile()).ToList());

            mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Guid id) =>
                    {
                        var prop = typeof(T).GetProperty("Id");
                        if (prop != null)
                        {
                            return items.FirstOrDefault(item =>
                                prop.GetValue(item)?.Equals(id) == true);
                        }
                        return null;
                    });

            mockRepo.Setup(repo => repo.AddAsync(It.IsAny<T>()))
                    .Callback<T>(item => items.Add(item))
                    .Returns(Task.CompletedTask);

            return mockRepo;
        }
    }
}