using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.API.Authorization;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Constants;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalonsController : ControllerBase
{
    private readonly ISalonService _salonService;
    private readonly IImageService _imageService;
    private readonly ILogger<SalonsController> _logger;

    public SalonsController(
        ISalonService salonService,
        IImageService imageService,
        ILogger<SalonsController> logger)
    {
        _salonService = salonService;
        _imageService = imageService;
        _logger = logger;
    }

    private Guid GetTenantId() => Guid.Parse(User.FindFirstValue("TenantId")!);
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Search salons with filters
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SalonSearchResultDto), 200)]
    public async Task<IActionResult> SearchSalons([FromQuery] SearchSalonDto dto)
    {
        try
        {
            var result = await _salonService.SearchSalonsAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching salons");
            return StatusCode(500, "An error occurred while searching salons");
        }
    }

    /// <summary>
    /// Get salon details by ID (public endpoint)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SalonDetailsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSalonDetails(Guid id)
    {
        try
        {
            var salon = await _salonService.GetSalonDetailsAsync(id);

            if (salon == null)
                return NotFound(new { message = "Salon not found" });

            return Ok(salon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salon details for ID {SalonId}", id);
            return StatusCode(500, "An error occurred while retrieving salon details");
        }
    }

    /// <summary>
    /// Get all salons for the current tenant
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.ViewSalons)]
    [ProducesResponseType(typeof(List<SalonDto>), 200)]
    public async Task<IActionResult> GetMySalons()
    {
        try
        {
            var tenantId = GetTenantId();
            var salons = await _salonService.GetSalonsByTenantIdAsync(tenantId);
            return Ok(salons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salons for tenant");
            return StatusCode(500, "An error occurred while retrieving salons");
        }
    }

    /// <summary>
    /// Create a new salon
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.CreateSalons)]
    [ProducesResponseType(typeof(SalonDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSalon([FromBody] CreateSalonDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            var salon = await _salonService.CreateSalonAsync(dto, tenantId);

            return CreatedAtAction(nameof(GetSalonDetails), new { id = salon.Id }, salon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating salon");
            return StatusCode(500, "An error occurred while creating the salon");
        }
    }

    /// <summary>
    /// Update an existing salon
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission(Permissions.EditSalons)]
    [ProducesResponseType(typeof(SalonDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSalon(Guid id, [FromBody] UpdateSalonDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest(new { message = "ID mismatch" });

            var tenantId = GetTenantId();
            var salon = await _salonService.UpdateSalonAsync(dto, tenantId);

            return Ok(salon);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Salon not found for update: {SalonId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized update attempt for salon: {SalonId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating salon {SalonId}", id);
            return StatusCode(500, "An error occurred while updating the salon");
        }
    }

    /// <summary>
    /// Delete a salon (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeleteSalons)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteSalon(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            await _salonService.DeleteSalonAsync(id, tenantId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Salon not found for deletion: {SalonId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized delete attempt for salon: {SalonId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting salon {SalonId}", id);
            return StatusCode(500, "An error occurred while deleting the salon");
        }
    }

    /// <summary>
    /// Upload image file to salon gallery
    /// </summary>
    [HttpPost("{id}/images/upload")]
    [HasPermission(Permissions.EditSalons)]
    [ProducesResponseType(typeof(SalonImageDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadSalonImageFile(Guid id, IFormFile file, [FromForm] bool isPrimary = false)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // Upload file to storage
            using var stream = file.OpenReadStream();
            var imageUrl = await _imageService.UploadImageAsync(stream, file.FileName, "salons");

            // Save to database
            var tenantId = GetTenantId();
            var image = await _salonService.UploadSalonImageAsync(id, tenantId, imageUrl, isPrimary);

            return CreatedAtAction(nameof(GetSalonDetails), new { id }, image);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image upload: {SalonId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized image upload attempt for salon: {SalonId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to salon {SalonId}", id);
            return StatusCode(500, "An error occurred while uploading the image");
        }
    }

    /// <summary>
    /// Upload image to salon gallery
    /// </summary>
    [HttpPost("{id}/images")]
    [HasPermission(Permissions.EditSalons)]
    [ProducesResponseType(typeof(SalonImageDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadSalonImage(Guid id, [FromBody] UploadImageDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = GetTenantId();
            var image = await _salonService.UploadSalonImageAsync(id, tenantId, dto.ImageUrl, dto.IsPrimary);

            return CreatedAtAction(nameof(GetSalonDetails), new { id }, image);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Salon not found for image upload: {SalonId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized image upload attempt for salon: {SalonId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to salon {SalonId}", id);
            return StatusCode(500, "An error occurred while uploading the image");
        }
    }

    /// <summary>
    /// Delete image from salon gallery
    /// </summary>
    [HttpDelete("images/{imageId}")]
    [HasPermission(Permissions.EditSalons)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteSalonImage(Guid imageId)
    {
        try
        {
            var tenantId = GetTenantId();
            await _salonService.DeleteSalonImageAsync(imageId, tenantId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Image not found for deletion: {ImageId}", imageId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized image delete attempt: {ImageId}", imageId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {ImageId}", imageId);
            return StatusCode(500, "An error occurred while deleting the image");
        }
    }

    /// <summary>
    /// Update salon business hours
    /// </summary>
    [HttpPut("{id}/business-hours")]
    [HasPermission(Permissions.EditSalons)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateBusinessHours(Guid id, [FromBody] UpdateBusinessHoursDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.SalonId)
                return BadRequest(new { message = "ID mismatch" });

            var tenantId = GetTenantId();
            await _salonService.UpdateBusinessHoursAsync(dto, tenantId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Salon not found for business hours update: {SalonId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized business hours update attempt for salon: {SalonId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business hours for salon {SalonId}", id);
            return StatusCode(500, "An error occurred while updating business hours");
        }
    }
}

public class UploadImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
