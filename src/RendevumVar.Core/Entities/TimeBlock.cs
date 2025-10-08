namespace RendevumVar.Core.Entities;

public class TimeBlock : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid StaffId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Reason { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
}
