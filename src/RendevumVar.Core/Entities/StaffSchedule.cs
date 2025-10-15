using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class StaffSchedule : BaseEntity
{
    public Guid StaffId { get; set; }
    public WorkDayOfWeek? DayOfWeek { get; set; }
    
    // Working Hours
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
    // Break Time (optional)
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    
    // Recurring or One-time
    public bool IsRecurring { get; set; } = true;
    public DateTime? SpecificDate { get; set; } // For one-time exceptions
    
    // Active/Inactive (for temporary schedule changes)
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public Staff Staff { get; set; } = null!;
}
