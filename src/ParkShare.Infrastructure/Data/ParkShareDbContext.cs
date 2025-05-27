using Microsoft.EntityFrameworkCore;
using ParkShare.Core.Entities; // Your entities namespace
using ParkShare.Core.Enums; // Your enums namespace

namespace ParkShare.Infrastructure.Data;

public class ParkShareDbContext : DbContext
{
    public ParkShareDbContext(DbContextOptions<ParkShareDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ParkingLot> ParkingLots { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<Availability> Availabilities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
        });

        // ParkingLot Configuration
        modelBuilder.Entity<ParkingLot>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Address).IsRequired().HasMaxLength(500);
            entity.Property(p => p.City).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Country).IsRequired().HasMaxLength(100);
            entity.Property(p => p.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(p => p.HourlyRate).HasColumnType("decimal(18,2)");

            // Relationship with User (Owner)
            entity.HasOne<User>() 
                  .WithMany() 
                  .HasForeignKey(p => p.OwnerId)
                  .IsRequired();

            // Relationship with Availability (One-to-Many)
            entity.HasMany(p => p.Availabilities) // Uses the ICollection<Availability> in ParkingLot
                  .WithOne() // Assumes Availability does not have a navigation property back to ParkingLot, or it's not configured here
                  .HasForeignKey(a => a.ParkingLotId)
                  .OnDelete(DeleteBehavior.Cascade); 
        });

        // Rental Configuration
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TotalCost).HasColumnType("decimal(18,2)");

            entity.Property(r => r.Status)
                  .HasConversion(
                      s => s.ToString(),
                      s => (RentalStatus)Enum.Parse(typeof(RentalStatus), s))
                  .HasMaxLength(50); 

            entity.HasOne<ParkingLot>() 
                  .WithMany() 
                  .HasForeignKey(r => r.ParkingLotId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict); 

            entity.HasOne<User>() 
                  .WithMany() 
                  .HasForeignKey(r => r.RenterId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict); 
        });

        // Availability Configuration
        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(a => a.Id);
            // Relationship to ParkingLot is defined in ParkingLot entity configuration using p.Availabilities
            // If Availability had a navigation property `ParkingLot ParkingLot { get; set; }`,
            // the .WithOne() in ParkingLot's config could be .WithOne(a => a.ParkingLot)
        });
    }
}
