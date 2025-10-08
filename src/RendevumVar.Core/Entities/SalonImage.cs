namespace RendevumVar.Core.Entities;

public class SalonImage : BaseEntity
{
    public Guid SalonId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public Salon Salon { get; set; } = null!;
}
