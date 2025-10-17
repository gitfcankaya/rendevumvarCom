using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Review?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Salon)
            .Include(r => r.Staff)
            .Include(r => r.ResponseByUser)
            .Include(r => r.Appointment)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<IEnumerable<Review>> GetBySalonIdAsync(Guid salonId, bool publishedOnly = true)
    {
        var query = _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Staff)
            .Include(r => r.ResponseByUser)
            .Where(r => r.SalonId == salonId && !r.IsDeleted);

        if (publishedOnly)
        {
            query = query.Where(r => r.IsPublished);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _dbSet
            .Include(r => r.Salon)
            .Include(r => r.Staff)
            .Include(r => r.ResponseByUser)
            .Where(r => r.CustomerId == customerId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByStaffIdAsync(Guid staffId, bool publishedOnly = true)
    {
        var query = _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Salon)
            .Include(r => r.ResponseByUser)
            .Where(r => r.StaffId == staffId && !r.IsDeleted);

        if (publishedOnly)
        {
            query = query.Where(r => r.IsPublished);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetByAppointmentIdAsync(Guid appointmentId)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Include(r => r.Salon)
            .Include(r => r.Staff)
            .Include(r => r.ResponseByUser)
            .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId && !r.IsDeleted);
    }

    public async Task<double> GetAverageRatingBySalonIdAsync(Guid salonId)
    {
        var reviews = await _dbSet
            .Where(r => r.SalonId == salonId && r.IsPublished && !r.IsDeleted)
            .ToListAsync();

        if (!reviews.Any())
            return 0;

        return reviews.Average(r => r.Rating);
    }

    public async Task<double> GetAverageRatingByStaffIdAsync(Guid staffId)
    {
        var reviews = await _dbSet
            .Where(r => r.StaffId == staffId && r.IsPublished && !r.IsDeleted)
            .ToListAsync();

        if (!reviews.Any())
            return 0;

        return reviews.Average(r => r.Rating);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionBySalonIdAsync(Guid salonId)
    {
        var reviews = await _dbSet
            .Where(r => r.SalonId == salonId && r.IsPublished && !r.IsDeleted)
            .ToListAsync();

        var distribution = new Dictionary<int, int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 }
        };

        foreach (var review in reviews)
        {
            if (review.Rating >= 1 && review.Rating <= 5)
            {
                distribution[review.Rating]++;
            }
        }

        return distribution;
    }

    public async Task<bool> HasCustomerReviewedAppointmentAsync(Guid appointmentId, Guid customerId)
    {
        return await _dbSet
            .AnyAsync(r => r.AppointmentId == appointmentId
                          && r.CustomerId == customerId
                          && !r.IsDeleted);
    }
}
