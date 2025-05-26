

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideAway.Domain.Entities;

namespace RideAway.Infrastructure.Persistence.EntityConfigurations
{
    public class RideConfiguration : IEntityTypeConfiguration<Ride>
    {
        public void Configure(EntityTypeBuilder<Ride> builder)
        {
            // Configure PickupLocation and Destination as strings
            builder.Property(r => r.PickupLocation)
                   .IsRequired()
                   .HasMaxLength(255); // Optional: Set a max length for the string column

            builder.Property(r => r.Destination)
                   .IsRequired()
                   .HasMaxLength(255); // Optional: Set a max length for the string column

            // Configure other properties
            builder.Property(r => r.Fare)
                   .IsRequired();

            builder.Property(r => r.Status)
                   .HasConversion<string>();
        }


    }

}
