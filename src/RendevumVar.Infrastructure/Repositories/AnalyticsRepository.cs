using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.DTOs;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(Guid? salonId, DateTime? startDate, DateTime? endDate)
    {
        var now = DateTime.UtcNow;
        var start = startDate ?? now.AddMonths(-1);
        var end = endDate ?? now;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var appointmentsQuery = _context.Appointments
            .Where(a => !a.IsDeleted && a.StartTime >= start && a.StartTime <= end);

        if (salonId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.SalonId == salonId.Value);

        var paymentsQuery = _context.Payments
            .Where(p => !p.IsDeleted && p.CreatedAt >= start && p.CreatedAt <= end && p.Status == PaymentStatus.Completed);

        if (salonId.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.Appointment != null && p.Appointment.SalonId == salonId.Value);

        // Revenue stats
        var totalRevenue = await paymentsQuery.SumAsync(p => p.Amount);
        var monthlyPayments = paymentsQuery.Where(p => p.CreatedAt >= monthStart);
        var monthlyRevenue = await monthlyPayments.SumAsync(p => p.Amount);

        var previousMonthStart = monthStart.AddMonths(-1);
        var previousMonthRevenue = await paymentsQuery
            .Where(p => p.CreatedAt >= previousMonthStart && p.CreatedAt < monthStart)
            .SumAsync(p => p.Amount);

        var revenueGrowth = previousMonthRevenue > 0
            ? ((monthlyRevenue - previousMonthRevenue) / previousMonthRevenue) * 100
            : 0;

        // Appointment stats
        var totalAppointments = await appointmentsQuery.CountAsync();
        var monthlyAppointments = await appointmentsQuery.Where(a => a.StartTime >= monthStart).CountAsync();
        var previousMonthAppointments = await appointmentsQuery
            .Where(a => a.StartTime >= previousMonthStart && a.StartTime < monthStart)
            .CountAsync();

        var appointmentGrowth = previousMonthAppointments > 0
            ? ((monthlyAppointments - previousMonthAppointments) * 100 / previousMonthAppointments)
            : 0;

        // Customer stats
        var customersQuery = _context.Users.Where(u => !u.IsDeleted && u.Role == UserRole.Customer);
        var totalCustomers = await customersQuery.CountAsync();
        var newCustomers = await customersQuery.Where(u => u.CreatedAt >= monthStart).CountAsync();
        var previousMonthCustomers = await customersQuery
            .Where(u => u.CreatedAt >= previousMonthStart && u.CreatedAt < monthStart)
            .CountAsync();

        var customerGrowth = previousMonthCustomers > 0
            ? (newCustomers - previousMonthCustomers) * 100 / previousMonthCustomers
            : 0;

        // Rating stats
        var reviewsQuery = _context.Reviews.Where(r => !r.IsDeleted);
        if (salonId.HasValue)
            reviewsQuery = reviewsQuery.Where(r => r.SalonId == salonId.Value);

        var avgRating = await reviewsQuery.AnyAsync() ? await reviewsQuery.AverageAsync(r => r.Rating) : 0;
        var totalReviews = await reviewsQuery.CountAsync();

        // Recent appointments
        var recentAppointments = await appointmentsQuery
            .OrderByDescending(a => a.StartTime)
            .Take(10)
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Select(a => new RecentAppointmentDto
            {
                Id = a.Id,
                StartTime = a.StartTime,
                CustomerName = a.Customer!.FirstName + " " + a.Customer.LastName,
                ServiceName = a.Service!.Name,
                StaffName = a.Staff!.FirstName + " " + a.Staff.LastName,
                Status = a.Status,
                TotalPrice = a.TotalPrice
            })
            .ToListAsync();

        // Top services
        var topServices = await appointmentsQuery
            .Where(a => a.Status == AppointmentStatus.Completed)
            .GroupBy(a => new { a.ServiceId, a.Service!.Name })
            .Select(g => new TopServiceDto
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.Name,
                BookingCount = g.Count(),
                Revenue = g.Sum(a => a.TotalPrice)
            })
            .OrderByDescending(s => s.Revenue)
            .Take(5)
            .ToListAsync();

        // Top staff
        var topStaff = await appointmentsQuery
            .Where(a => a.Status == AppointmentStatus.Completed)
            .GroupBy(a => new { a.StaffId, a.Staff!.FirstName, a.Staff.LastName, a.Staff.PhotoUrl })
            .Select(g => new TopStaffDto
            {
                StaffId = g.Key.StaffId,
                StaffName = g.Key.FirstName + " " + g.Key.LastName,
                PhotoUrl = g.Key.PhotoUrl,
                AppointmentCount = g.Count(),
                Revenue = g.Sum(a => a.TotalPrice),
                AverageRating = _context.Reviews
                    .Where(r => r.StaffId == g.Key.StaffId && !r.IsDeleted)
                    .Average(r => (decimal?)r.Rating) ?? 0
            })
            .OrderByDescending(s => s.Revenue)
            .Take(5)
            .ToListAsync();

        return new DashboardAnalyticsDto
        {
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            RevenueGrowthPercentage = revenueGrowth,

            TotalAppointments = totalAppointments,
            MonthlyAppointments = monthlyAppointments,
            AppointmentGrowthPercentage = appointmentGrowth,

            TotalCustomers = totalCustomers,
            NewCustomersThisMonth = newCustomers,
            CustomerGrowthPercentage = customerGrowth,

            AverageRating = avgRating,
            TotalReviews = totalReviews,

            RecentAppointments = recentAppointments,
            TopServices = topServices,
            TopStaff = topStaff
        };
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        var paymentsQuery = _context.Payments
            .Where(p => !p.IsDeleted && p.CreatedAt >= startDate && p.CreatedAt <= endDate);

        if (salonId.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.Appointment != null && p.Appointment.SalonId == salonId.Value);

        var completedPayments = paymentsQuery.Where(p => p.Status == PaymentStatus.Completed);
        var totalRevenue = await completedPayments.SumAsync(p => p.Amount);
        var totalRefunds = await completedPayments.SumAsync(p => p.RefundAmount ?? 0);
        var transactionCount = await completedPayments.CountAsync();

        var dailyBreakdown = await GetDailyRevenueAsync(startDate, endDate, salonId);
        var serviceRevenue = await GetRevenueByServiceAsync(startDate, endDate, salonId);
        var paymentMethodRevenue = await GetRevenueByPaymentMethodAsync(startDate, endDate, salonId);

        return new RevenueReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = totalRevenue,
            TotalRefunds = totalRefunds,
            NetRevenue = totalRevenue - totalRefunds,
            TotalTransactions = transactionCount,
            AverageTransactionValue = transactionCount > 0 ? totalRevenue / transactionCount : 0,
            DailyBreakdown = dailyBreakdown,
            RevenueByService = serviceRevenue.Select(s => new RevenueByServiceDto
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Revenue = s.Revenue,
                BookingCount = s.BookingCount,
                PercentageOfTotal = s.Percentage
            }).ToList(),
            RevenueByPaymentMethod = paymentMethodRevenue.Select(p => new RevenueByPaymentMethodDto
            {
                PaymentMethod = p.PaymentMethod,
                PaymentMethodName = p.PaymentMethodName,
                Amount = p.Amount,
                Revenue = p.Revenue,
                TransactionCount = p.TransactionCount,
                PercentageOfTotal = p.Percentage
            }).ToList()
        };
    }

    public async Task<List<DailyRevenueDto>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        var paymentsQuery = _context.Payments
            .Where(p => !p.IsDeleted && p.Status == PaymentStatus.Completed
                && p.CreatedAt >= startDate && p.CreatedAt <= endDate);

        if (salonId.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.Appointment != null && p.Appointment.SalonId == salonId.Value);

        return await paymentsQuery
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Sum(p => p.Amount),
                TransactionCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();
    }

    public async Task<List<ServiceRevenueDto>> GetRevenueByServiceAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        var appointmentsQuery = _context.Appointments
            .Where(a => !a.IsDeleted && a.Status == AppointmentStatus.Completed
                && a.StartTime >= startDate && a.StartTime <= endDate);

        if (salonId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.SalonId == salonId.Value);

        var serviceRevenues = await appointmentsQuery
            .GroupBy(a => new { a.ServiceId, a.Service!.Name })
            .Select(g => new ServiceRevenueDto
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.Name,
                Revenue = g.Sum(a => a.TotalPrice),
                BookingCount = g.Count()
            })
            .ToListAsync();

        var totalRevenue = serviceRevenues.Sum(s => s.Revenue);
        foreach (var service in serviceRevenues)
        {
            service.Percentage = totalRevenue > 0 ? (service.Revenue / totalRevenue) * 100 : 0;
        }

        return serviceRevenues.OrderByDescending(s => s.Revenue).ToList();
    }

    public async Task<List<PaymentMethodRevenueDto>> GetRevenueByPaymentMethodAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        var paymentsQuery = _context.Payments
            .Where(p => !p.IsDeleted && p.Status == PaymentStatus.Completed
                && p.CreatedAt >= startDate && p.CreatedAt <= endDate);

        if (salonId.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.Appointment != null && p.Appointment.SalonId == salonId.Value);

        var methodRevenues = await paymentsQuery
            .GroupBy(p => p.Method)
            .Select(g => new PaymentMethodRevenueDto
            {
                PaymentMethod = g.Key,
                PaymentMethodName = g.Key.ToString(),
                Revenue = g.Sum(p => p.Amount),
                Amount = g.Sum(p => p.Amount),
                TransactionCount = g.Count()
            })
            .ToListAsync();

        var totalRevenue = methodRevenues.Sum(m => m.Revenue);
        foreach (var method in methodRevenues)
        {
            method.Percentage = totalRevenue > 0 ? (method.Revenue / totalRevenue) * 100 : 0;
        }

        return methodRevenues.OrderByDescending(m => m.Revenue).ToList();
    }

    public async Task<AppointmentAnalyticsDto> GetAppointmentAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        var appointmentsQuery = _context.Appointments
            .Where(a => !a.IsDeleted && a.StartTime >= startDate && a.StartTime <= endDate);

        if (salonId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.SalonId == salonId.Value);

        var total = await appointmentsQuery.CountAsync();
        var completed = await appointmentsQuery.CountAsync(a => a.Status == AppointmentStatus.Completed);
        var cancelled = await appointmentsQuery.CountAsync(a => a.Status == AppointmentStatus.Cancelled);
        var noShow = await appointmentsQuery.CountAsync(a => a.Status == AppointmentStatus.NoShow);

        var statusBreakdown = await appointmentsQuery
            .GroupBy(a => a.Status)
            .Select(g => new AppointmentStatusBreakdownDto
            {
                Status = g.Key,
                StatusName = g.Key.ToString(),
                Count = g.Count(),
                Percentage = total > 0 ? (g.Count() * 100.0m / total) : 0
            })
            .ToListAsync();

        var hourlyDist = await GetHourlyDistributionAsync(startDate, endDate, salonId);
        var dayOfWeekDist = await GetDayOfWeekDistributionAsync(startDate, endDate, salonId);

        return new AppointmentAnalyticsDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalAppointments = total,
            CompletedAppointments = completed,
            CancelledAppointments = cancelled,
            NoShowAppointments = noShow,
            CompletionRate = total > 0 ? (completed * 100.0m / total) : 0,
            CancellationRate = total > 0 ? (cancelled * 100.0m / total) : 0,
            NoShowRate = total > 0 ? (noShow * 100.0m / total) : 0,
            StatusBreakdown = statusBreakdown,
            HourlyDistribution = hourlyDist.Select(h => new HourlyDistributionDto
            {
                Hour = h.Hour,
                AppointmentCount = h.Count
            }).ToList(),
            DayOfWeekDistribution = dayOfWeekDist.Select(d => new DayOfWeekDistributionDto
            {
                DayOfWeek = d.DayOfWeek,
                DayName = d.DayName,
                AppointmentCount = d.Count,
                AveragePerWeek = 0 // Calculate if needed
            }).ToList()
        };
    }

    public async Task<List<HourlyAppointmentDto>> GetHourlyDistributionAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        var appointmentsQuery = _context.Appointments
            .Where(a => !a.IsDeleted && a.StartTime >= startDate && a.StartTime <= endDate);

        if (salonId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.SalonId == salonId.Value);

        return await appointmentsQuery
            .GroupBy(a => a.StartTime.Hour)
            .Select(g => new HourlyAppointmentDto
            {
                Hour = g.Key,
                Count = g.Count()
            })
            .OrderBy(h => h.Hour)
            .ToListAsync();
    }

    public async Task<List<DayOfWeekAppointmentDto>> GetDayOfWeekDistributionAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        var appointmentsQuery = _context.Appointments
            .Where(a => !a.IsDeleted && a.StartTime >= startDate && a.StartTime <= endDate);

        if (salonId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.SalonId == salonId.Value);

        return await appointmentsQuery
            .GroupBy(a => a.StartTime.DayOfWeek)
            .Select(g => new DayOfWeekAppointmentDto
            {
                DayOfWeek = g.Key,
                DayName = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderBy(d => d.DayOfWeek)
            .ToListAsync();
    }

    public async Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(DateTime startDate, DateTime endDate, Guid? salonId, Guid? staffId)
    {
        var staffQuery = _context.Staff.Where(s => !s.IsDeleted && s.Status == StaffStatus.Active);

        if (salonId.HasValue)
            staffQuery = staffQuery.Where(s => s.SalonId == salonId.Value);

        if (staffId.HasValue)
            staffQuery = staffQuery.Where(s => s.Id == staffId.Value);

        var staff = await staffQuery.ToListAsync();
        var performances = new List<StaffPerformanceDto>();

        foreach (var member in staff)
        {
            var performance = await GetStaffPerformanceByIdAsync(member.Id, startDate, endDate);
            if (performance != null)
                performances.Add(performance);
        }

        return performances.OrderByDescending(p => p.TotalRevenue).ToList();
    }

    public async Task<StaffPerformanceDto?> GetStaffPerformanceByIdAsync(Guid staffId, DateTime startDate, DateTime endDate)
    {
        var staff = await _context.Staff
            .Where(s => s.Id == staffId && !s.IsDeleted)
            .FirstOrDefaultAsync();

        if (staff == null)
            return null;

        var appointments = await _context.Appointments
            .Where(a => a.StaffId == staffId && !a.IsDeleted
                && a.StartTime >= startDate && a.StartTime <= endDate)
            .ToListAsync();

        var total = appointments.Count;
        var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var totalRevenue = appointments.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => a.TotalPrice);

        var reviews = await _context.Reviews
            .Where(r => r.StaffId == staffId && !r.IsDeleted)
            .ToListAsync();

        var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        return new StaffPerformanceDto
        {
            StaffId = staffId,
            StaffName = $"{staff.FirstName} {staff.LastName}",
            PhotoUrl = staff.PhotoUrl,
            TotalAppointments = total,
            CompletedAppointments = completed,
            CompletionRate = total > 0 ? (completed * 100.0m / total) : 0,
            TotalRevenue = totalRevenue,
            AverageRevenuePerAppointment = completed > 0 ? totalRevenue / completed : 0,
            AverageRating = (decimal)avgRating,
            TotalReviews = reviews.Count,
            ReviewCount = reviews.Count,
            TotalWorkingHours = 0, // Could be calculated from schedules
            RevenuePerHour = 0 // Could be calculated
        };
    }

    public async Task<CustomerInsightsDto> GetCustomerInsightsAsync(DateTime startDate, DateTime endDate, Guid? salonId)
    {
        var customersQuery = _context.Users.Where(u => !u.IsDeleted && u.Role == UserRole.Customer);
        var totalCustomers = await customersQuery.CountAsync();

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var newCustomers = await customersQuery.Where(u => u.CreatedAt >= monthStart).CountAsync();

        var appointmentsQuery = _context.Appointments
            .Where(a => !a.IsDeleted && a.StartTime >= startDate && a.StartTime <= endDate);

        if (salonId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.SalonId == salonId.Value);

        var returningCustomers = await appointmentsQuery
            .GroupBy(a => a.CustomerId)
            .Where(g => g.Count() > 1)
            .CountAsync();

        var retentionRate = totalCustomers > 0 ? (returningCustomers * 100.0m / totalCustomers) : 0;

        var paymentsQuery = _context.Payments
            .Where(p => !p.IsDeleted && p.Status == PaymentStatus.Completed);

        var avgLifetimeValue = totalCustomers > 0
            ? await paymentsQuery.SumAsync(p => p.Amount) / totalCustomers
            : 0;

        var avgBookings = totalCustomers > 0
            ? await appointmentsQuery.CountAsync() / (decimal)totalCustomers
            : 0;

        var topCustomers = await GetTopCustomersAsync(startDate, endDate, salonId, 10);

        return new CustomerInsightsDto
        {
            TotalCustomers = totalCustomers,
            NewCustomersThisMonth = newCustomers,
            ReturningCustomers = returningCustomers,
            CustomerRetentionRate = retentionRate,
            AverageLifetimeValue = avgLifetimeValue,
            AverageBookingsPerCustomer = avgBookings,
            TopCustomers = topCustomers,
            CustomerSegments = new List<CustomerSegmentDto>() // Could implement segments
        };
    }

    public async Task<List<TopCustomerDto>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, Guid? salonId, int topCount = 10)
    {
        var appointmentsQuery = _context.Appointments
            .Where(a => !a.IsDeleted && a.Status == AppointmentStatus.Completed
                && a.StartTime >= startDate && a.StartTime <= endDate);

        if (salonId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.SalonId == salonId.Value);

        return await appointmentsQuery
            .GroupBy(a => new { a.CustomerId, a.Customer!.FirstName, a.Customer.LastName, a.Customer.Email })
            .Select(g => new TopCustomerDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.FirstName + " " + g.Key.LastName,
                Email = g.Key.Email,
                TotalBookings = g.Count(),
                TotalSpent = g.Sum(a => a.TotalPrice),
                LastBookingDate = g.Max(a => a.StartTime)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate, DateTime? endDate, Guid? salonId)
    {
        var query = _context.Payments.Where(p => !p.IsDeleted && p.Status == PaymentStatus.Completed);

        if (startDate.HasValue)
            query = query.Where(p => p.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.CreatedAt <= endDate.Value);

        if (salonId.HasValue)
            query = query.Where(p => p.Appointment != null && p.Appointment.SalonId == salonId.Value);

        return await query.SumAsync(p => p.Amount);
    }

    public async Task<int> GetTotalAppointmentsAsync(DateTime? startDate, DateTime? endDate, Guid? salonId)
    {
        var query = _context.Appointments.Where(a => !a.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(a => a.StartTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.StartTime <= endDate.Value);

        if (salonId.HasValue)
            query = query.Where(a => a.SalonId == salonId.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetTotalCustomersAsync(Guid? salonId)
    {
        var query = _context.Users.Where(u => !u.IsDeleted && u.Role == UserRole.Customer);

        if (salonId.HasValue)
        {
            var customerIds = await _context.Appointments
                .Where(a => a.SalonId == salonId.Value && !a.IsDeleted)
                .Select(a => a.CustomerId)
                .Distinct()
                .ToListAsync();

            query = query.Where(u => customerIds.Contains(u.Id));
        }

        return await query.CountAsync();
    }

    public async Task<decimal> GetAverageSalonRatingAsync(Guid salonId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.SalonId == salonId && !r.IsDeleted)
            .ToListAsync();

        return reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;
    }
}
