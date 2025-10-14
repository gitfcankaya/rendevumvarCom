using System.ComponentModel.DataAnnotations;

namespace RendevumVar.Core.Entities;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = "System";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    // Audit Helper Methods
    public void SetCreated(string userId)
    {
        CreatedBy = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetUpdated(string userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDeleted(string userId)
    {
        IsDeleted = true;
        DeletedBy = userId;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore(string userId)
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = null;
        SetUpdated(userId);
    }
}
