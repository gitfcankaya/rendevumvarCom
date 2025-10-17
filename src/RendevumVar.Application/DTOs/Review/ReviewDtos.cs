namespace RendevumVar.Application.DTOs.Review;

public class CreateReviewDto
{
    public Guid AppointmentId { get; set; }
    public Guid SalonId { get; set; }
    public Guid? StaffId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}

public class ReviewResponseDto
{
    public string Response { get; set; } = string.Empty;
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid SalonId { get; set; }
    public string SalonName { get; set; } = string.Empty;
    public Guid? StaffId { get; set; }
    public string? StaffName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? Response { get; set; }
    public Guid? ResponseBy { get; set; }
    public string? ResponseByName { get; set; }
    public DateTime? ResponseAt { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SalonRatingDto
{
    public Guid SalonId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}

public class StaffRatingDto
{
    public Guid StaffId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
