using RendevumVar.Core.Entities;

namespace RendevumVar.Core.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Review>> GetBySalonIdAsync(Guid salonId, bool publishedOnly = true);
    Task<IEnumerable<Review>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Review>> GetByStaffIdAsync(Guid staffId, bool publishedOnly = true);
    Task<Review?> GetByAppointmentIdAsync(Guid appointmentId);
    Task<double> GetAverageRatingBySalonIdAsync(Guid salonId);
    Task<double> GetAverageRatingByStaffIdAsync(Guid staffId);
    Task<Dictionary<int, int>> GetRatingDistributionBySalonIdAsync(Guid salonId);
    Task<bool> HasCustomerReviewedAppointmentAsync(Guid appointmentId, Guid customerId);
}
