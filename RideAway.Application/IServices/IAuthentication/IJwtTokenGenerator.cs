using RideAway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.IServices.IAuthentication
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
