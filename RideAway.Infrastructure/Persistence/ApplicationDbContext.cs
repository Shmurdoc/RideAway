using Microsoft.EntityFrameworkCore;
using RideAway.Application.IRepositories;
using RideAway.Domain.Entities;
using RideAway.Domain.Value_Object;
using RideAway.Infrastructure.Persistence.EntityConfigurations;

namespace RideAway.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Ride> Rides { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .Property(u => u.CurrentLocation)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .HasMaxLength(256);

            // Emails are normalised to lower-case on write. The index stops duplicates
            // under concurrent registration; the app check is the friendly error.
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(256);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            // Ride
            modelBuilder.Entity<Ride>()
                .Property(r => r.Fare)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Rider)
                .WithMany()
                .HasForeignKey(r => r.RiderId)
                .OnDelete(DeleteBehavior.Restrict); // deleting a rider must not erase paid rides

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Driver)
                .WithMany()
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict for Driver relationship to avoid cycles

            // Payment
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18, 4)");

            // One settlement per ride. Filter derives from the enum so reordering it
            // cannot silently break the constraint.
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.RideId)
                .IsUnique()
                .HasFilter($"[RideId] IS NOT NULL AND [Status] = {(int)PaymentStatus.Completed}");

            // Apply configurations from separate entity configuration classes
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }

}
