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

            // Configure User entity
            modelBuilder.Entity<User>()
                .Property(u => u.CurrentLocation)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .HasMaxLength(256);

            // Emails are normalised to lower-case on write, so the uniqueness check is
            // case-insensitive by construction. The unique index is what actually
            // prevents duplicate accounts under concurrent registration; the
            // application-level check is only a friendly-error path.
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(256);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            // Configure Ride entity
            modelBuilder.Entity<Ride>()
                .Property(r => r.Fare)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Rider)
                .WithMany()
                .HasForeignKey(r => r.RiderId)
                // Never cascade: deleting a rider must not erase completed, paid rides.
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Driver)
                .WithMany()
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict for Driver relationship to avoid cycles

            // Configure Payment entity
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18, 4)");

            // A ride can be settled at most once. This is the database-level guarantee
            // that a replayed or concurrent payment request cannot double-charge.
            // PaymentStatus is persisted as an int, so the filter is derived from the
            // enum rather than hard-coded - otherwise reordering the enum would
            // silently stop the constraint from matching anything.
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.RideId)
                .IsUnique()
                .HasFilter($"[RideId] IS NOT NULL AND [Status] = {(int)PaymentStatus.Completed}");

            // Apply configurations from separate entity configuration classes
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }

}
