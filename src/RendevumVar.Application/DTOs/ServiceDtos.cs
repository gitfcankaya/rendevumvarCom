using System.ComponentModel.DataAnnotations;

namespace RendevumVar.Application.DTOs;

public class ServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string? Category { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public List<Guid>? StaffIds { get; set; }
}

public class CreateServiceDto
{
    [Required(ErrorMessage = "Hizmet adı gereklidir")]
    [StringLength(200, ErrorMessage = "Hizmet adı en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Fiyat gereklidir")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Süre gereklidir")]
    [Range(1, 1440, ErrorMessage = "Süre 1-1440 dakika arasında olmalıdır")]
    public int DurationMinutes { get; set; } = 60;

    [StringLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }

    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    public string? ImageUrl { get; set; }
}

public class UpdateServiceDto
{
    [Required(ErrorMessage = "Hizmet adı gereklidir")]
    [StringLength(200, ErrorMessage = "Hizmet adı en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Fiyat gereklidir")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Süre gereklidir")]
    [Range(1, 1440, ErrorMessage = "Süre 1-1440 dakika arasında olmalıdır")]
    public int DurationMinutes { get; set; }

    [StringLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }

    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ServiceListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Computed properties
    public string PriceDisplay => Price.ToString("C");
    public string DurationDisplay => $"{DurationMinutes} dk";
    public string StatusDisplay => IsActive ? "Aktif" : "Pasif";
}