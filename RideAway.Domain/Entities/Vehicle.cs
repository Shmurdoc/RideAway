using RideAway.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideAway.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public Guid DriverId { get; set; }
        public string? Model { get; set; } = string.Empty;
        public string? PlateNumber { get; set; } = string.Empty;
        public RideCategory Category { get; set; } = RideCategory.Standard; // Standard, Luxury, SUV
    }
}
