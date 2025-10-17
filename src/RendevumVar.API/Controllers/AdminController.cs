using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.DTOs;
using RendevumVar.Core.Enums;
using RendevumVar.Infrastructure.Data;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region User Management

    /// <summary>
    /// Get all users with filtering and pagination
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<PagedResultDto<AdminUserListDto>>> GetUsers([FromQuery] UserFilterDto filter)
    {
        try
        {
            var query = _context.Users.Where(u => !u.IsDeleted).AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchLower) ||
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    (u.Phone != null && u.Phone.Contains(searchLower))
                );
            }

            if (filter.Role.HasValue)
                query = query.Where(u => u.Role == filter.Role.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (filter.EmailConfirmed.HasValue)
                query = query.Where(u => u.EmailConfirmed == filter.EmailConfirmed.Value);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "email" => filter.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "firstname" => filter.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                "lastname" => filter.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                "role" => filter.SortDescending ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
                "lastlogin" => filter.SortDescending ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
                _ => filter.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };

            // Apply pagination
            var users = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new AdminUserListDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return Ok(new PagedResultDto<AdminUserListDto>
            {
                Items = users,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users list");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get user details by ID
    /// </summary>
    [HttpGet("users/{id}")]
    public async Task<ActionResult<AdminUserDetailDto>> GetUser(Guid id)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == id && !u.IsDeleted)
                .Select(u => new AdminUserDetailDto
                {
                    Id = u.Id,
                    TenantId = u.TenantId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.Phone,
                    Role = u.Role,
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    EmailConfirmed = u.EmailConfirmed,
                    PhoneConfirmed = u.PhoneConfirmed,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    TotalAppointments = u.Appointments.Count(a => !a.IsDeleted),
                    TotalReviews = u.Reviews.Count(r => !r.IsDeleted),
                    TotalSpent = u.Payments.Where(p => !p.IsDeleted && p.Status == PaymentStatus.Completed).Sum(p => p.Amount)
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound($"User with ID {id} not found");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user details for {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving user details");
        }
    }

    /// <summary>
    /// Update user role
    /// </summary>
    [HttpPut("users/{id}/role")]
    public async Task<ActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Prevent self-role change
            if (user.Id == adminId)
                return BadRequest("You cannot change your own role");

            user.Role = dto.Role;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} updated role for user {UserId} to {Role}", adminId, id, dto.Role);

            return Ok(new { message = "User role updated successfully", role = dto.Role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for {UserId}", id);
            return StatusCode(500, "An error occurred while updating user role");
        }
    }

    /// <summary>
    /// Soft delete user
    /// </summary>
    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Prevent self-deletion
            if (user.Id == adminId)
                return BadRequest("You cannot delete your own account");

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = false;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} deleted user {UserId}", adminId, id);

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting user");
        }
    }

    #endregion

    #region Salon Approval

    /// <summary>
    /// Get pending salons awaiting approval
    /// </summary>
    [HttpGet("salons/pending")]
    public async Task<ActionResult<List<PendingSalonDto>>> GetPendingSalons()
    {
        try
        {
            var salons = await _context.Salons
                .Include(s => s.Tenant)
                .Where(s => !s.IsDeleted && s.Status == SalonStatus.Pending)
                .OrderBy(s => s.CreatedAt)
                .Select(s => new PendingSalonDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Address = s.Address,
                    City = s.City,
                    Phone = s.Phone,
                    Email = s.Email,
                    OwnerId = s.Tenant.OwnerId,
                    OwnerName = s.Tenant.Owner!.FirstName + " " + s.Tenant.Owner.LastName,
                    OwnerEmail = s.Tenant.Owner!.Email,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    ApprovedAt = s.ApprovedAt,
                    RejectionReason = s.RejectionReason
                })
                .ToListAsync();

            return Ok(salons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending salons");
            return StatusCode(500, "An error occurred while retrieving pending salons");
        }
    }

    /// <summary>
    /// Approve a salon
    /// </summary>
    [HttpPut("salons/{id}/approve")]
    public async Task<ActionResult> ApproveSalon(Guid id, [FromBody] ApproveSalonDto dto)
    {
        try
        {
            var salon = await _context.Salons.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (salon == null)
                return NotFound($"Salon with ID {id} not found");

            if (salon.Status != SalonStatus.Pending)
                return BadRequest($"Salon is not in pending status (current: {salon.Status})");

            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            salon.Status = SalonStatus.Approved;
            salon.ApprovedAt = DateTime.UtcNow;
            salon.ApprovedBy = adminId;
            salon.IsActive = true;
            salon.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} approved salon {SalonId}", adminId, id);

            return Ok(new { message = "Salon approved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving salon {SalonId}", id);
            return StatusCode(500, "An error occurred while approving salon");
        }
    }

    /// <summary>
    /// Reject a salon
    /// </summary>
    [HttpPut("salons/{id}/reject")]
    public async Task<ActionResult> RejectSalon(Guid id, [FromBody] RejectSalonDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                return BadRequest("Rejection reason is required");

            var salon = await _context.Salons.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (salon == null)
                return NotFound($"Salon with ID {id} not found");

            if (salon.Status != SalonStatus.Pending)
                return BadRequest($"Salon is not in pending status (current: {salon.Status})");

            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            salon.Status = SalonStatus.Rejected;
            salon.RejectionReason = dto.RejectionReason;
            salon.ApprovedBy = adminId;
            salon.IsActive = false;
            salon.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} rejected salon {SalonId} with reason: {Reason}", adminId, id, dto.RejectionReason);

            return Ok(new { message = "Salon rejected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting salon {SalonId}", id);
            return StatusCode(500, "An error occurred while rejecting salon");
        }
    }

    #endregion
}
