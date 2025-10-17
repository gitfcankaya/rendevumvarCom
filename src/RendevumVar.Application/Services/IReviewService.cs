using RendevumVar.Application.DTOs.Review;

namespace RendevumVar.Application.Services;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, Guid customerId);
    Task<ReviewDto> UpdateReviewAsync(Guid id, UpdateReviewDto dto, Guid customerId);
    Task DeleteReviewAsync(Guid id, Guid customerId);
    Task<ReviewDto?> GetReviewByIdAsync(Guid id);
    Task<IEnumerable<ReviewDto>> GetReviewsBySalonIdAsync(Guid salonId, bool publishedOnly = true);
    Task<IEnumerable<ReviewDto>> GetReviewsByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<ReviewDto>> GetReviewsByStaffIdAsync(Guid staffId, bool publishedOnly = true);
    Task<ReviewDto?> GetReviewByAppointmentIdAsync(Guid appointmentId);
    Task<SalonRatingDto> GetSalonRatingAsync(Guid salonId);
    Task<StaffRatingDto> GetStaffRatingAsync(Guid staffId);
    Task<ReviewDto> AddResponseAsync(Guid reviewId, ReviewResponseDto dto, Guid userId);
    Task<ReviewDto> TogglePublishAsync(Guid reviewId, Guid salonOwnerId);
}
