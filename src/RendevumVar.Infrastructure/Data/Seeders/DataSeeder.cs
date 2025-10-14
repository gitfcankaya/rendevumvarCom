using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Data.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await SeedServiceCategories(context);
            await SeedDefaultTenant(context);
            await context.SaveChangesAsync();
        }

        private static async Task SeedServiceCategories(ApplicationDbContext context)
        {
            if (!await context.ServiceCategories.AnyAsync())
            {
                var categories = new List<ServiceCategory>
                {
                    new ServiceCategory
                    {
                        TenantId = null, // Global category
                        Name = "Saç Bakımı",
                        Description = "Saç kesimi, boyama ve şekillendirme hizmetleri",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ServiceCategory
                    {
                        TenantId = null, // Global category
                        Name = "Cilt Bakımı",
                        Description = "Yüz bakımı ve cilt temizleme hizmetleri",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ServiceCategory
                    {
                        TenantId = null, // Global category
                        Name = "Makyaj",
                        Description = "Profesyonel makyaj hizmetleri",
                        DisplayOrder = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ServiceCategory
                    {
                        TenantId = null, // Global category
                        Name = "Tırnak Bakımı",
                        Description = "Manikür ve pedikür hizmetleri",
                        DisplayOrder = 4,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ServiceCategory
                    {
                        TenantId = null, // Global category
                        Name = "Kaş Bakımı",
                        Description = "Kaş alma ve şekillendirme hizmetleri",
                        DisplayOrder = 5,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.ServiceCategories.AddRange(categories);
            }
        }

        private static async Task SeedDefaultTenant(ApplicationDbContext context)
        {
            if (!await context.Tenants.AnyAsync())
            {
                var tenant = new Tenant
                {
                    Name = "Demo Salon",
                    Subdomain = "demo",
                    SubscriptionPlan = "Premium",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                context.Tenants.Add(tenant);
                await context.SaveChangesAsync();

                // Add demo salon
                var salon = new Salon
                {
                    TenantId = tenant.Id,
                    Name = "Demo Beauty Salon",
                    Address = "Demo Adres",
                    City = "İstanbul",
                    Phone = "+90 555 123 4567",
                    Email = "demo@demosalon.com",
                    Description = "Güzellik ve bakım hizmetleri",
                    BusinessHours = @"{""monday"":{""open"":""09:00"",""close"":""18:00""}}",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Salons.Add(salon);
                await context.SaveChangesAsync();

                // Add demo user
                var user = new User
                {
                    TenantId = tenant.Id,
                    Email = "admin@demosalon.com",
                    FirstName = "Demo",
                    LastName = "Admin",
                    Phone = "+90 555 123 4567",
                    Role = UserRole.BusinessOwner,
                    PasswordHash = "$2a$11$L8z8qQ5y8rZHGbzHx7VgqOBK1qvd0lN9iO0mU1xK3Y4kzH9P2rL6e", // "Admin123!"
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                // Get service categories
                var hairCategory = await context.ServiceCategories.FirstOrDefaultAsync(c => c.Name == "Saç Bakımı");
                var nailCategory = await context.ServiceCategories.FirstOrDefaultAsync(c => c.Name == "Tırnak Bakımı");

                if (hairCategory != null && nailCategory != null)
                {
                    // Add demo services
                    var services = new List<Service>
                    {
                        new Service
                        {
                            TenantId = tenant.Id,
                            SalonId = salon.Id,
                            CategoryId = hairCategory.Id,
                            Name = "Kadın Saç Kesimi",
                            Description = "Profesyonel kadın saç kesimi ve şekillendirme",
                            DurationMinutes = 60,
                            Price = 150m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Service
                        {
                            TenantId = tenant.Id,
                            SalonId = salon.Id,
                            CategoryId = hairCategory.Id,
                            Name = "Saç Boyama",
                            Description = "Saç boyama ve bakım hizmeti",
                            DurationMinutes = 120,
                            Price = 300m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Service
                        {
                            TenantId = tenant.Id,
                            SalonId = salon.Id,
                            CategoryId = nailCategory.Id,
                            Name = "Manikür",
                            Description = "El ve tırnak bakımı",
                            DurationMinutes = 45,
                            Price = 100m,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.Services.AddRange(services);
                }
            }
        }
    }
}