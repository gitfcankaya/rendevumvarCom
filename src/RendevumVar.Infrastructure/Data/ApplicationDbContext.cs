using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;

namespace RendevumVar.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Salon> Salons { get; set; }
    public DbSet<SalonImage> SalonImages { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<TimeBlock> TimeBlocks { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant Configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.SubscriptionPlan).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Role);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RefreshToken Configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Salon Configuration
        modelBuilder.Entity<Salon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Salons)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SalonImage Configuration
        modelBuilder.Entity<SalonImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.SalonId);

            entity.HasOne(e => e.Salon)
                .WithMany(s => s.Images)
                .HasForeignKey(e => e.SalonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ServiceCategory Configuration
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.TenantId);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Service Configuration
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SalonId);
            entity.HasIndex(e => e.CategoryId);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Salon)
                .WithMany(s => s.Services)
                .HasForeignKey(e => e.SalonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Many-to-many relationship with Staff
            entity.HasMany(e => e.Staff)
                .WithMany(s => s.Services)
                .UsingEntity(j => j.ToTable("StaffServices"));
        });

        // Staff Configuration
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SalonId);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Salon)
                .WithMany(s => s.Staff)
                .HasForeignKey(e => e.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Appointment Configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
            entity.Property(e => e.DepositPaid).HasPrecision(10, 2);
            entity.Property(e => e.CustomerNotes).HasMaxLength(500);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SalonId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.StaffId);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Salon)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                .WithMany(u => u.Appointments)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TimeBlock Configuration
        modelBuilder.Entity<TimeBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(200);
            entity.Property(e => e.RecurrencePattern).HasMaxLength(100);
            entity.HasIndex(e => e.StaffId);
            entity.HasIndex(e => e.StartTime);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.TimeBlocks)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Review Configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.Response).HasMaxLength(1000);
            entity.HasIndex(e => e.AppointmentId);
            entity.HasIndex(e => e.SalonId);
            entity.HasIndex(e => e.StaffId);
            entity.HasIndex(e => e.CustomerId);

            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.Review)
                .HasForeignKey<Review>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Salon)
                .WithMany(s => s.Reviews)
                .HasForeignKey(e => e.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.Reviews)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ResponseByUser)
                .WithMany()
                .HasForeignKey(e => e.ResponseBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.RefundAmount).HasPrecision(10, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TransactionId).HasMaxLength(200);
            entity.Property(e => e.PaymentGateway).HasMaxLength(50);
            entity.HasIndex(e => e.AppointmentId);
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Appointment)
                .WithMany(a => a.Payments)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Add check constraints
        modelBuilder.Entity<Review>()
            .ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "Rating >= 1 AND Rating <= 5"));

        modelBuilder.Entity<Service>()
            .ToTable(t => 
            {
                t.HasCheckConstraint("CK_Service_Duration", "DurationMinutes > 0");
                t.HasCheckConstraint("CK_Service_Price", "Price >= 0");
            });
    }
}
