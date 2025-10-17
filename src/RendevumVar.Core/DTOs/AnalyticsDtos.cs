using RendevumVar.Core.Enums;

namespace RendevumVar.Core.DTOs;

public class DashboardAnalyticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal RevenueGrowthPercentage { get; set; }
    public int TotalAppointments { get; set; }
    public int MonthlyAppointments { get; set; }
    public decimal AppointmentGrowthPercentage { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public decimal CustomerGrowthPercentage { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<TopServiceDto> TopServices { get; set; } = new();
    public List<TopStaffDto> TopStaff { get; set; } = new();
    public List<RecentAppointmentDto> RecentAppointments { get; set; } = new();
}

public class TopServiceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class TopStaffDto
{
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int AppointmentCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
}

public class RecentAppointmentDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
}

public class RevenueReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal NetRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
    public List<RevenueByServiceDto> RevenueByService { get; set; } = new();
    public List<RevenueByPaymentMethodDto> RevenueByPaymentMethod { get; set; } = new();
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}

public class RevenueByServiceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class RevenueByPaymentMethodDto
{
    public string PaymentMethodName { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Revenue { get; set; }
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class AppointmentAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShowAppointments { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal NoShowRate { get; set; }
    public List<AppointmentStatusBreakdownDto> StatusBreakdown { get; set; } = new();
    public List<HourlyDistributionDto> HourlyDistribution { get; set; } = new();
    public List<DayOfWeekDistributionDto> DayOfWeekDistribution { get; set; } = new();
}

public class AppointmentStatusBreakdownDto
{
    public AppointmentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class HourlyDistributionDto
{
    public int Hour { get; set; }
    public int AppointmentCount { get; set; }
}

public class DayOfWeekDistributionDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public decimal AveragePerWeek { get; set; }
}

public class StaffPerformanceDto
{
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenuePerAppointment { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int ReviewCount { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public decimal RevenuePerHour { get; set; }
    public List<ServicePerformanceDto> ServiceBreakdown { get; set; } = new();
}

public class ServicePerformanceDto
{
    public string ServiceName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class CustomerInsightsDto
{
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal CustomerRetentionRate { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public decimal AverageAppointmentsPerCustomer { get; set; }
    public decimal AverageBookingsPerCustomer { get; set; }
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
    public List<CustomerAcquisitionDto> MonthlyAcquisition { get; set; } = new();
    public List<CustomerSegmentDto> CustomerSegments { get; set; } = new();
}

public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastAppointment { get; set; }
    public DateTime LastBookingDate { get; set; }
}

public class CustomerAcquisitionDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int NewCustomers { get; set; }
}

public class CustomerSegmentDto
{
    public string SegmentName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

// Additional DTOs for Repository methods
public class ServiceRevenueDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal Percentage { get; set; }
}

public class PaymentMethodRevenueDto
{
    public string PaymentMethodName { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Revenue { get; set; }
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}

public class HourlyAppointmentDto
{
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class DayOfWeekAppointmentDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int Count { get; set; }
}
