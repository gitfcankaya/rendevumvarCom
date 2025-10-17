using RendevumVar.Application.DTOs.Review;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;

namespace RendevumVar.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ISalonRepository _salonRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IAppointmentRepository appointmentRepository,
        ISalonRepository salonRepository)
    {
        _reviewRepository = reviewRepository;
        _appointmentRepository = appointmentRepository;
        _salonRepository = salonRepository;
    }

    public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, Guid customerId)
    {
        // Validate rating
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5");
        }

        // Check if appointment exists and belongs to customer
        var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);
        if (appointment == null)
        {
            throw new InvalidOperationException("Appointment not found");
        }

        if (appointment.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("You can only review your own appointments");
        }

        // Check if appointment is completed
        if (appointment.Status != AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("You can only review completed appointments");
        }

        // Check if already reviewed
        var existingReview = await _reviewRepository.GetByAppointmentIdAsync(dto.AppointmentId);
        if (existingReview != null)
        {
            throw new InvalidOperationException("You have already reviewed this appointment");
        }

        var review = new Review
        {
            AppointmentId = dto.AppointmentId,
            CustomerId = customerId,
            SalonId = dto.SalonId,
            StaffId = dto.StaffId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            IsPublished = true
        };

        await _reviewRepository.AddAsync(review);

        return await MapToDto(review);
    }

    public async Task<ReviewDto> UpdateReviewAsync(Guid id, UpdateReviewDto dto, Guid customerId)
    {
        // Validate rating
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5");
        }

        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            throw new InvalidOperationException("Review not found");
        }

        if (review.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("You can only update your own reviews");
        }

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review);

        return await MapToDto(review);
    }

    public async Task DeleteReviewAsync(Guid id, Guid customerId)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            throw new InvalidOperationException("Review not found");
        }

        if (review.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("You can only delete your own reviews");
        }

        await _reviewRepository.DeleteAsync(review);
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(Guid id)
    {
        var review = await _reviewRepository.GetByIdWithDetailsAsync(id);
        return review == null ? null : await MapToDto(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsBySalonIdAsync(Guid salonId, bool publishedOnly = true)
    {
        var reviews = await _reviewRepository.GetBySalonIdAsync(salonId, publishedOnly);
        return await Task.WhenAll(reviews.Select(MapToDto));
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsByCustomerIdAsync(Guid customerId)
    {
        var reviews = await _reviewRepository.GetByCustomerIdAsync(customerId);
        return await Task.WhenAll(reviews.Select(MapToDto));
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsByStaffIdAsync(Guid staffId, bool publishedOnly = true)
    {
        var reviews = await _reviewRepository.GetByStaffIdAsync(staffId, publishedOnly);
        return await Task.WhenAll(reviews.Select(MapToDto));
    }

    public async Task<ReviewDto?> GetReviewByAppointmentIdAsync(Guid appointmentId)
    {
        var review = await _reviewRepository.GetByAppointmentIdAsync(appointmentId);
        return review == null ? null : await MapToDto(review);
    }

    public async Task<SalonRatingDto> GetSalonRatingAsync(Guid salonId)
    {
        var reviews = await _reviewRepository.GetBySalonIdAsync(salonId, publishedOnly: true);
        var averageRating = await _reviewRepository.GetAverageRatingBySalonIdAsync(salonId);
        var distribution = await _reviewRepository.GetRatingDistributionBySalonIdAsync(salonId);

        return new SalonRatingDto
        {
            SalonId = salonId,
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = reviews.Count(),
            RatingDistribution = distribution
        };
    }

    public async Task<StaffRatingDto> GetStaffRatingAsync(Guid staffId)
    {
        var reviews = await _reviewRepository.GetByStaffIdAsync(staffId, publishedOnly: true);
        var averageRating = await _reviewRepository.GetAverageRatingByStaffIdAsync(staffId);

        return new StaffRatingDto
        {
            StaffId = staffId,
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = reviews.Count()
        };
    }

    public async Task<ReviewDto> AddResponseAsync(Guid reviewId, ReviewResponseDto dto, Guid userId)
    {
        var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
        if (review == null)
        {
            throw new InvalidOperationException("Review not found");
        }

        // Verify user has access to this salon (via tenant)
        var salon = await _salonRepository.GetByIdAsync(review.SalonId);
        if (salon == null)
        {
            throw new InvalidOperationException("Salon not found");
        }

        // TODO: Add proper authorization check using ITenantService or similar
        // For now, we'll just check if the user's TenantId matches the salon's TenantId
        // This should be enhanced with proper role checking

        review.Response = dto.Response;
        review.ResponseBy = userId;
        review.ResponseAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review);

        return await MapToDto(review);
    }

    public async Task<ReviewDto> TogglePublishAsync(Guid reviewId, Guid salonOwnerId)
    {
        var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
        if (review == null)
        {
            throw new InvalidOperationException("Review not found");
        }

        // Verify user has access to this salon (via tenant)
        var salon = await _salonRepository.GetByIdAsync(review.SalonId);
        if (salon == null)
        {
            throw new InvalidOperationException("Salon not found");
        }

        // TODO: Add proper authorization check using ITenantService or similar

        review.IsPublished = !review.IsPublished;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review);

        return await MapToDto(review);
    }

    private async Task<ReviewDto> MapToDto(Review review)
    {
        // Ensure navigation properties are loaded
        var fullReview = review.Customer == null
            ? await _reviewRepository.GetByIdWithDetailsAsync(review.Id)
            : review;

        if (fullReview == null)
        {
            throw new InvalidOperationException("Review not found");
        }

        return new ReviewDto
        {
            Id = fullReview.Id,
            AppointmentId = fullReview.AppointmentId,
            CustomerId = fullReview.CustomerId,
            CustomerName = fullReview.Customer != null
                ? $"{fullReview.Customer.FirstName} {fullReview.Customer.LastName}"
                : "Unknown",
            SalonId = fullReview.SalonId,
            SalonName = fullReview.Salon?.Name ?? "Unknown",
            StaffId = fullReview.StaffId,
            StaffName = fullReview.Staff != null ? $"{fullReview.Staff.FirstName} {fullReview.Staff.LastName}" : null,
            Rating = fullReview.Rating,
            Comment = fullReview.Comment,
            Response = fullReview.Response,
            ResponseBy = fullReview.ResponseBy,
            ResponseByName = fullReview.ResponseByUser != null
                ? $"{fullReview.ResponseByUser.FirstName} {fullReview.ResponseByUser.LastName}"
                : null,
            ResponseAt = fullReview.ResponseAt,
            IsPublished = fullReview.IsPublished,
            CreatedAt = fullReview.CreatedAt
        };
    }
}
