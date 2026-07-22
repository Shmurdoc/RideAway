using RideAway.Domain.Entities;
using RideAway.Domain.Entities.Enum;
using RideAway.Domain.Value_Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Application.DTOs
{

    public class CreateUserDTO
    {
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; }  // Enum: Rider, Driver, Admin
    }

    public class UserDTO
    {
        public Guid Id { get; set; }
    }

    public class DriverLocationUpdateDTO : UserDTO
    {
        public string? CurrentLocation { get; set; } = null!;
    }

}
