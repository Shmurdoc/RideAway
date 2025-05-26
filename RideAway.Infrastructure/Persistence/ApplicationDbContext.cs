using Microsoft.EntityFrameworkCore;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using RideAway.Infrastructure.Persistence.EntityConfigurations;

namespace RideAway.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Ride> Rides { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .Property(u => u.CurrentLocation)
                .HasMaxLength(255);

            // Configure Ride entity
            modelBuilder.Entity<Ride>()
                .Property(r => r.Fare)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Rider)
                .WithMany()
                .HasForeignKey(r => r.RiderId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade only for Rider relationship

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Driver)
                .WithMany()
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict for Driver relationship to avoid cycles

            // Configure Payment entity
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18, 4)");

            // Apply configurations from separate entity configuration classes
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }

}
