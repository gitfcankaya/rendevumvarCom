namespace RendevumVar.Application.DTOs;

public class WorkingHoursDto
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public List<BreakTimeDto> Breaks { get; set; } = new();
    public bool IsWorkingDay { get; set; } = true;
}

public class BreakTimeDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Reason { get; set; }
}
