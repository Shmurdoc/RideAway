using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RideAway.Domain.Entities.Enum;

namespace RideAway.Domain.Entities
{
    public class User : BaseEntity
    {
        public string? Name { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } // Rider, Driver, Admin
        public string? PhoneNumber { get; set; } = string.Empty;
        public Vehicle? Vehicle { get; set; } // Only for drivers
        public string? CurrentLocation { get; set; } = null!; // only for drivers
    }
}
