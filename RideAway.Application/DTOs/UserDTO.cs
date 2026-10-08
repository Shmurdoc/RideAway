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
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>Only <see cref="UserRole.Driver"/> is honoured; anything else registers as Rider.</summary>
        public UserRole Role { get; set; }
    }

    public class UserDTO
    {
        public Guid Id { get; set; }
    }

    /// <summary>What the API returns for a user. Has no password field by design.</summary>
    public class UserProfileDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
    }

    public class DriverLocationUpdateDTO : UserDTO
    {
        public string? CurrentLocation { get; set; } = null!;
    }

}
