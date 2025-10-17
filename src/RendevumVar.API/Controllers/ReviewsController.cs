using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.Application.DTOs.Review;
using RendevumVar.Application.Services;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Create a new review for an appointment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto dto)
    {
        try
        {
            var customerId = GetUserId();
            var review = await _reviewService.CreateReviewAsync(dto, customerId);
            return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing review
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid id, [FromBody] UpdateReviewDto dto)
    {
        try
        {
            var customerId = GetUserId();
            var review = await _reviewService.UpdateReviewAsync(id, dto, customerId);
            return Ok(review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> DeleteReview(Guid id)
    {
        try
        {
            var customerId = GetUserId();
            await _reviewService.DeleteReviewAsync(id, customerId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Get a specific review by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReviewById(Guid id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null)
        {
            return NotFound(new { message = "Review not found" });
        }
        return Ok(review);
    }

    /// <summary>
    /// Get all reviews for a salon
    /// </summary>
    [HttpGet("salon/{salonId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsBySalon(
        Guid salonId,
        [FromQuery] bool includeUnpublished = false)
    {
        var publishedOnly = !includeUnpublished || !User.Identity?.IsAuthenticated == true;
        var reviews = await _reviewService.GetReviewsBySalonIdAsync(salonId, publishedOnly);
        return Ok(reviews);
    }

    /// <summary>
    /// Get all reviews by the current customer
    /// </summary>
    [HttpGet("my-reviews")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetMyReviews()
    {
        var customerId = GetUserId();
        var reviews = await _reviewService.GetReviewsByCustomerIdAsync(customerId);
        return Ok(reviews);
    }

    /// <summary>
    /// Get all reviews for a staff member
    /// </summary>
    [HttpGet("staff/{staffId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByStaff(
        Guid staffId,
        [FromQuery] bool includeUnpublished = false)
    {
        var publishedOnly = !includeUnpublished || !User.Identity?.IsAuthenticated == true;
        var reviews = await _reviewService.GetReviewsByStaffIdAsync(staffId, publishedOnly);
        return Ok(reviews);
    }

    /// <summary>
    /// Get review for a specific appointment
    /// </summary>
    [HttpGet("appointment/{appointmentId}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> GetReviewByAppointment(Guid appointmentId)
    {
        var review = await _reviewService.GetReviewByAppointmentIdAsync(appointmentId);
        if (review == null)
        {
            return NotFound(new { message = "No review found for this appointment" });
        }
        return Ok(review);
    }

    /// <summary>
    /// Get rating statistics for a salon
    /// </summary>
    [HttpGet("salon/{salonId}/rating")]
    public async Task<ActionResult<SalonRatingDto>> GetSalonRating(Guid salonId)
    {
        var rating = await _reviewService.GetSalonRatingAsync(salonId);
        return Ok(rating);
    }

    /// <summary>
    /// Get rating statistics for a staff member
    /// </summary>
    [HttpGet("staff/{staffId}/rating")]
    public async Task<ActionResult<StaffRatingDto>> GetStaffRating(Guid staffId)
    {
        var rating = await _reviewService.GetStaffRatingAsync(staffId);
        return Ok(rating);
    }

    /// <summary>
    /// Add a response to a review (salon owner only)
    /// </summary>
    [HttpPost("{id}/response")]
    [Authorize(Roles = "SalonOwner")]
    public async Task<ActionResult<ReviewDto>> AddResponse(Guid id, [FromBody] ReviewResponseDto dto)
    {
        try
        {
            var userId = GetUserId();
            var review = await _reviewService.AddResponseAsync(id, dto, userId);
            return Ok(review);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Toggle publish status of a review (salon owner only)
    /// </summary>
    [HttpPatch("{id}/toggle-publish")]
    [Authorize(Roles = "SalonOwner")]
    public async Task<ActionResult<ReviewDto>> TogglePublish(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var review = await _reviewService.TogglePublishAsync(id, userId);
            return Ok(review);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
