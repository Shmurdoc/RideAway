using MediatR;
using RideAway.Application.Features.Rides.Queries;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.Features.Rides.Handlers.Queries
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, User>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.UserRepository.GetByIdAsync(request.Guid);
        }
    }
}
