using RendevumVar.Core.Entities;

namespace RendevumVar.Core.Entities;

public class ContentPage : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public string? ImageUrl { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
}