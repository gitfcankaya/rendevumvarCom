using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.Core.DTOs;
using RendevumVar.Application.Services;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard analytics overview
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "SalonOwner,Admin")]
    public async Task<ActionResult<DashboardAnalyticsDto>> GetDashboard([FromQuery] Guid? salonId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var result = await _analyticsService.GetDashboardAnalyticsAsync(salonId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard analytics");
            return StatusCode(500, new { message = "Error retrieving dashboard analytics" });
        }
    }

    /// <summary>
    /// Get revenue report with detailed breakdown
    /// </summary>
    [HttpGet("revenue")]
    [Authorize(Roles = "SalonOwner,Admin")]
    public async Task<ActionResult<RevenueReportDto>> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid? salonId)
    {
        try
        {
            var result = await _analyticsService.GetRevenueReportAsync(startDate, endDate, salonId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving revenue report");
            return StatusCode(500, new { message = "Error retrieving revenue report" });
        }
    }

    /// <summary>
    /// Get appointment analytics with status breakdown and distributions
    /// </summary>
    [HttpGet("appointments")]
    [Authorize(Roles = "SalonOwner,Admin")]
    public async Task<ActionResult<AppointmentAnalyticsDto>> GetAppointmentAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid? salonId)
    {
        try
        {
            var result = await _analyticsService.GetAppointmentAnalyticsAsync(startDate, endDate, salonId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment analytics");
            return StatusCode(500, new { message = "Error retrieving appointment analytics" });
        }
    }

    /// <summary>
    /// Get staff performance metrics
    /// </summary>
    [HttpGet("staff-performance")]
    [Authorize(Roles = "SalonOwner,Admin")]
    public async Task<ActionResult<List<StaffPerformanceDto>>> GetStaffPerformance([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid? salonId, [FromQuery] Guid? staffId)
    {
        try
        {
            var result = await _analyticsService.GetStaffPerformanceAsync(startDate, endDate, salonId, staffId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff performance");
            return StatusCode(500, new { message = "Error retrieving staff performance" });
        }
    }

    /// <summary>
    /// Get customer insights and behavior analytics
    /// </summary>
    [HttpGet("customers")]
    [Authorize(Roles = "SalonOwner,Admin")]
    public async Task<ActionResult<CustomerInsightsDto>> GetCustomerInsights([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid? salonId)
    {
        try
        {
            var result = await _analyticsService.GetCustomerInsightsAsync(startDate, endDate, salonId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer insights");
            return StatusCode(500, new { message = "Error retrieving customer insights" });
        }
    }
}
