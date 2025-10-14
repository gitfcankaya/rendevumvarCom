using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceCategoriesController : ControllerBase
{
    private readonly IServiceCategoryService _categoryService;
    private readonly ILogger<ServiceCategoriesController> _logger;

    public ServiceCategoriesController(
        IServiceCategoryService categoryService,
        ILogger<ServiceCategoriesController> logger)
    {
        _categoryService = categoryService;
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
    /// Tüm kategorileri getirir (aktif + pasif)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var categories = await _categoryService.GetAllCategoriesAsync(tenantId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategoriler getirilirken hata oluştu");
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Sadece aktif kategorileri getirir
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCategories()
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var categories = await _categoryService.GetActiveCategoriesAsync(tenantId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktif kategoriler getirilirken hata oluştu");
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Belirli bir kategoriyi ID ile getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var category = await _categoryService.GetCategoryByIdAsync(id, tenantId);
            if (category == null)
            {
                return NotFound("Kategori bulunamadı");
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori getirilirken hata oluştu. CategoryId: {CategoryId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Yeni kategori oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateServiceCategoryDto dto)
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
            var category = await _categoryService.CreateCategoryAsync(dto, tenantId, userId);
            
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori oluşturulurken hata oluştu");
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Mevcut kategoriyi günceller
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateServiceCategoryDto dto)
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
            var category = await _categoryService.UpdateCategoryAsync(id, dto, tenantId, userId);
            
            if (category == null)
            {
                return NotFound("Kategori bulunamadı");
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori güncellenirken hata oluştu. CategoryId: {CategoryId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Kategoriyi soft delete ile siler
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var userId = GetUserId();
            var result = await _categoryService.DeleteCategoryAsync(id, tenantId, userId);
            
            if (!result)
            {
                return NotFound("Kategori bulunamadı");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Kategori silinemedi. CategoryId: {CategoryId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori silinirken hata oluştu. CategoryId: {CategoryId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Silinen kategoriyi geri yükler
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreCategory(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var userId = GetUserId();
            var result = await _categoryService.RestoreCategoryAsync(id, tenantId, userId);
            
            if (!result)
            {
                return NotFound("Silinmiş kategori bulunamadı");
            }

            return Ok("Kategori başarıyla geri yüklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori geri yüklenirken hata oluştu. CategoryId: {CategoryId}", id);
            return StatusCode(500, "Bir hata oluştu");
        }
    }

    /// <summary>
    /// Kategorilerin sıralamasını günceller
    /// </summary>
    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderCategories([FromBody] Dictionary<Guid, int> orderMap)
    {
        try
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Geçersiz tenant bilgisi");
            }

            var userId = GetUserId();
            await _categoryService.ReorderCategoriesAsync(orderMap, tenantId, userId);
            
            return Ok("Sıralama güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori sıralaması güncellenirken hata oluştu");
            return StatusCode(500, "Bir hata oluştu");
        }
    }
}