using RendevumVar.Core.DTOs;
using RendevumVar.Core.Repositories;

namespace RendevumVar.Application.Services;

public interface IAnalyticsService
{
    Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(Guid? salonId, DateTime? startDate, DateTime? endDate);
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate, Guid? salonId);
    Task<AppointmentAnalyticsDto> GetAppointmentAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? salonId);
    Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(DateTime startDate, DateTime endDate, Guid? salonId, Guid? staffId);
    Task<CustomerInsightsDto> GetCustomerInsightsAsync(DateTime startDate, DateTime endDate, Guid? salonId);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public AnalyticsService(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(Guid? salonId, DateTime? startDate, DateTime? endDate)
    {
        return await _analyticsRepository.GetDashboardAnalyticsAsync(salonId, startDate, endDate);
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before end date");

        return await _analyticsRepository.GetRevenueReportAsync(startDate, endDate, salonId);
    }

    public async Task<AppointmentAnalyticsDto> GetAppointmentAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before end date");

        return await _analyticsRepository.GetAppointmentAnalyticsAsync(startDate, endDate, salonId);
    }

    public async Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(DateTime startDate, DateTime endDate, Guid? salonId, Guid? staffId)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before end date");

        return await _analyticsRepository.GetStaffPerformanceAsync(startDate, endDate, salonId, staffId);
    }

    public async Task<CustomerInsightsDto> GetCustomerInsightsAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before end date");

        return await _analyticsRepository.GetCustomerInsightsAsync(startDate, endDate, salonId);
    }
}
