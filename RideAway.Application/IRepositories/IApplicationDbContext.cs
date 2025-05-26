using RideAway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace RideAway.Application.IRepositories
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Ride> Rides { get; set; }
        DbSet<Payment> Payments { get; set; }
        DbSet<Vehicle> Vehicles { get; set; }

    }
}
