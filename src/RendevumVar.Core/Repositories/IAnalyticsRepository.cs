using RendevumVar.Core.DTOs;
using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Repositories;

public interface IAnalyticsRepository
{
    // Dashboard Analytics
    Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(Guid? salonId, DateTime? startDate, DateTime? endDate);

    // Revenue Analytics
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate, Guid? salonId);
    Task<List<DailyRevenueDto>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate, Guid? salonId);
    Task<List<ServiceRevenueDto>> GetRevenueByServiceAsync(DateTime startDate, DateTime endDate, Guid? salonId);
    Task<List<PaymentMethodRevenueDto>> GetRevenueByPaymentMethodAsync(DateTime startDate, DateTime endDate, Guid? salonId);

    // Appointment Analytics
    Task<AppointmentAnalyticsDto> GetAppointmentAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? salonId);
    Task<List<HourlyAppointmentDto>> GetHourlyDistributionAsync(DateTime startDate, DateTime endDate, Guid? salonId);
    Task<List<DayOfWeekAppointmentDto>> GetDayOfWeekDistributionAsync(DateTime startDate, DateTime endDate, Guid? salonId);

    // Staff Performance
    Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(DateTime startDate, DateTime endDate, Guid? salonId, Guid? staffId);
    Task<StaffPerformanceDto?> GetStaffPerformanceByIdAsync(Guid staffId, DateTime startDate, DateTime endDate);

    // Customer Insights
    Task<CustomerInsightsDto> GetCustomerInsightsAsync(DateTime startDate, DateTime endDate, Guid? salonId);
    Task<List<TopCustomerDto>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, Guid? salonId, int topCount = 10);

    // Quick Stats
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate, DateTime? endDate, Guid? salonId);
    Task<int> GetTotalAppointmentsAsync(DateTime? startDate, DateTime? endDate, Guid? salonId);
    Task<int> GetTotalCustomersAsync(Guid? salonId);
    Task<decimal> GetAverageSalonRatingAsync(Guid salonId);
}
