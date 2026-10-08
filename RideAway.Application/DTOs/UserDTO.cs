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

        /// <summary>
        /// Requested account type. Ignored unless it is <see cref="UserRole.Driver"/>;
        /// <see cref="UserRole.Admin"/> can never be self-assigned. Drivers need an
        /// elevated account, so the field stays, but it is not a privilege grant.
        /// </summary>
        public UserRole Role { get; set; }
    }

    public class UserDTO
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Safe representation of a user for API responses. Deliberately has no
    /// PasswordHash member, so it cannot be leaked by serializing the wrong type.
    /// </summary>
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
