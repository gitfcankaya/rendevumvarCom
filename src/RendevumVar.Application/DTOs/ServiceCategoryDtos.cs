using System.ComponentModel.DataAnnotations;

namespace RendevumVar.Application.DTOs;

public class ServiceCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ServiceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CreateServiceCategoryDto
{
    [Required(ErrorMessage = "Kategori adı gereklidir")]
    [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    public string? Description { get; set; }

    [Range(0, 9999, ErrorMessage = "Sıra numarası 0-9999 arasında olmalıdır")]
    public int DisplayOrder { get; set; } = 0;
}

public class UpdateServiceCategoryDto
{
    [Required(ErrorMessage = "Kategori adı gereklidir")]
    [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    public string? Description { get; set; }

    [Range(0, 9999, ErrorMessage = "Sıra numarası 0-9999 arasında olmalıdır")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ServiceCategoryListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ServiceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string StatusDisplay => IsActive ? "Aktif" : "Pasif";
}