using RendevumVar.Core.Enums;

namespace RendevumVar.Application.DTOs;

public class CreateAppointmentDto
{
    public Guid SalonId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid StaffId { get; set; }
    public DateTime StartTime { get; set; }
    public string? Notes { get; set; }
    public string? CustomerNotes { get; set; }
}

public class UpdateAppointmentDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public Guid? StaffId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentStatusDto
{
    public AppointmentStatus Status { get; set; }
    public string? CancellationReason { get; set; }
}

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SalonId { get; set; }
    public string SalonName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int ServiceDuration { get; set; }
    public decimal ServicePrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? CustomerNotes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DepositPaid { get; set; }
    public bool ReminderSent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentDetailsDto : AppointmentDto
{
    public SalonDto Salon { get; set; } = null!;
    public ServiceDto Service { get; set; } = null!;
    public StaffDto Staff { get; set; } = null!;
    public List<PaymentDto> Payments { get; set; } = new();
}

public class AppointmentSearchDto
{
    public Guid? SalonId { get; set; }
    public Guid? StaffId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public AppointmentStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

