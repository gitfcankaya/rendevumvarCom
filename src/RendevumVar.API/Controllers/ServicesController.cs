using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(IServiceService serviceService, ILogger<ServicesController> logger)
    {
        _serviceService = serviceService;
        _logger = logger;
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : Guid.Empty;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
    }

    /// <summary>
    /// Tüm hizmetleri getirir (aktif + pasif)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllServices()
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var services = await _serviceService.GetAllServicesAsync(tenantId);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hizmetler getirilirken hata oluştu");
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Sadece aktif hizmetleri getirir
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveServices()
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var services = await _serviceService.GetActiveServicesAsync(tenantId);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktif hizmetler getirilirken hata oluştu");
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Belirli bir hizmeti ID ile getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetService(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var service = await _serviceService.GetServiceByIdAsync(id, tenantId);
            if (service == null)
            {
                return NotFound("Hizmet bulunamadı");
            }

            return Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hizmet getirilirken hata oluştu. ServiceId: {ServiceId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Yeni hizmet oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var userId = GetUserId();
            var service = await _serviceService.CreateServiceAsync(dto, tenantId, userId);
            
            return CreatedAtAction(nameof(GetService), new { id = service.Id }, service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hizmet oluşturulurken hata oluştu");
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Mevcut hizmeti günceller
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var userId = GetUserId();
            var service = await _serviceService.UpdateServiceAsync(id, dto, tenantId, userId);
            
            if (service == null)
            {
                return NotFound("Hizmet bulunamadı");
            }

            return Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hizmet güncellenirken hata oluştu. ServiceId: {ServiceId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Hizmeti soft delete ile siler
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var userId = GetUserId();
            var result = await _serviceService.DeleteServiceAsync(id, tenantId, userId);
            
            if (!result)
            {
                return NotFound("Hizmet bulunamadı");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hizmet silinirken hata oluştu. ServiceId: {ServiceId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Silinen hizmeti geri yükler
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreService(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var userId = GetUserId();
            var result = await _serviceService.RestoreServiceAsync(id, tenantId, userId);
            
            if (!result)
            {
                return NotFound("Silinmiş hizmet bulunamadı");
            }

            return Ok("Hizmet başarıyla geri yüklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hizmet geri yüklenirken hata oluştu. ServiceId: {ServiceId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Kategoriye göre hizmetleri getirir
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetServicesByCategory(string category)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var services = await _serviceService.GetServicesByCategoryAsync(category, tenantId);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategoriye göre hizmetler getirilirken hata oluştu. Category: {Category}", category);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Tüm kategorileri getirir
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var categories = await _serviceService.GetCategoriesAsync(tenantId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategoriler getirilirken hata oluştu");
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Get services by salon ID
    /// </summary>
    [HttpGet("salon/{salonId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServicesBySalon(Guid salonId)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                // For public access, get services without tenant filtering in service layer
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var services = await _serviceService.GetServicesBySalonIdAsync(salonId, tenantId);
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting services for salon {SalonId}", salonId);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Get service with assigned staff
    /// </summary>
    [HttpGet("{id}/staff")]
    public async Task<IActionResult> GetServiceWithStaff(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var service = await _serviceService.GetServiceWithStaffAsync(id, tenantId);
            if (service == null)
            {
                return NotFound("Hizmet bulunamadı");
            }

            return Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service with staff. ServiceId: {ServiceId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Assign staff to service
    /// </summary>
    [HttpPost("{serviceId}/staff/{staffId}")]
    public async Task<IActionResult> AssignStaffToService(Guid serviceId, Guid staffId)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            await _serviceService.AssignStaffToServiceAsync(serviceId, staffId, tenantId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error assigning staff {StaffId} to service {ServiceId}", staffId, serviceId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning staff {StaffId} to service {ServiceId}", staffId, serviceId);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Remove staff from service
    /// </summary>
    [HttpDelete("{serviceId}/staff/{staffId}")]
    public async Task<IActionResult> RemoveStaffFromService(Guid serviceId, Guid staffId)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            await _serviceService.RemoveStaffFromServiceAsync(serviceId, staffId, tenantId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error removing staff {StaffId} from service {ServiceId}", staffId, serviceId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing staff {StaffId} from service {ServiceId}", staffId, serviceId);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Get staff assigned to a service
    /// </summary>
    [HttpGet("{serviceId}/staff/list")]
    public async Task<IActionResult> GetServiceStaff(Guid serviceId)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var staff = await _serviceService.GetServiceStaffAsync(serviceId, tenantId);
            return Ok(staff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff for service {ServiceId}", serviceId);
            return StatusCode(500, "Bir hata oluştu");
        }
    }
}