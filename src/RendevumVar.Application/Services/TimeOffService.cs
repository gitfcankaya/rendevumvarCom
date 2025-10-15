using Microsoft.Extensions.Logging;
using RendevumVar.Application.DTOs;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;

namespace RendevumVar.Application.Services;

public class TimeOffService : ITimeOffService
{
    private readonly ITimeOffRequestRepository _timeOffRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<TimeOffService> _logger;

    public TimeOffService(
        ITimeOffRequestRepository timeOffRepository,
        IStaffRepository staffRepository,
        IEmailService emailService,
        ILogger<TimeOffService> logger)
    {
        _timeOffRepository = timeOffRepository;
        _staffRepository = staffRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<TimeOffRequestDto> RequestTimeOffAsync(
        Guid staffId,
        CreateTimeOffRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            throw new KeyNotFoundException("Personel bulunamadı.");
        }

        // Validate dates
        if (dto.StartDate >= dto.EndDate)
        {
            throw new InvalidOperationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        if (dto.StartDate.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Geçmiş tarih için izin talebi oluşturamazsınız.");
        }

        // Check for conflicting time off requests
        var conflicts = await _timeOffRepository.GetConflictingRequestsAsync(
            staffId,
            dto.StartDate,
            dto.EndDate,
            cancellationToken);

        if (conflicts.Any())
        {
            throw new InvalidOperationException("Bu tarih aralığında mevcut bir izin talebiniz var.");
        }

        var timeOffRequest = new TimeOffRequest
        {
            Id = Guid.NewGuid(),
            StaffId = staffId,
            TenantId = staff.TenantId,
            Type = dto.Type,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            Status = TimeOffStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = staff.Email
        };

        await _timeOffRepository.AddAsync(timeOffRequest);

        _logger.LogInformation("Time off request created for staff {StaffId}", staffId);

        return await MapToTimeOffDto(timeOffRequest);
    }

    public async Task<IEnumerable<TimeOffRequestDto>> GetStaffTimeOffAsync(
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        var requests = await _timeOffRepository.GetByStaffIdAsync(staffId);
        
        var dtos = new List<TimeOffRequestDto>();
        foreach (var request in requests)
        {
            dtos.Add(await MapToTimeOffDto(request));
        }

        return dtos;
    }

    public async Task<IEnumerable<TimeOffRequestDto>> GetPendingRequestsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var requests = await _timeOffRepository.GetPendingRequestsAsync(tenantId);
        
        var dtos = new List<TimeOffRequestDto>();
        foreach (var request in requests)
        {
            dtos.Add(await MapToTimeOffDto(request));
        }

        return dtos;
    }

    public async Task<TimeOffRequestDto> ApproveTimeOffRequestAsync(
        Guid requestId,
        Guid approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        var request = await _timeOffRepository.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new KeyNotFoundException("İzin talebi bulunamadı.");
        }

        if (request.Status != TimeOffStatus.Pending)
        {
            throw new InvalidOperationException("Sadece bekleyen talepler onaylanabilir.");
        }

        request.Status = TimeOffStatus.Approved;
        request.ApprovedByUserId = approvedByUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await _timeOffRepository.UpdateAsync(request);

        // Send notification email
        var staff = await _staffRepository.GetByIdAsync(request.StaffId);
        if (staff != null)
        {
            await _emailService.SendTimeOffApprovedAsync(
                staff.Email,
                staff.FullName,
                request.StartDate,
                request.EndDate,
                cancellationToken);
        }

        _logger.LogInformation("Time off request {RequestId} approved", requestId);

        return await MapToTimeOffDto(request);
    }

    public async Task<TimeOffRequestDto> RejectTimeOffRequestAsync(
        Guid requestId,
        Guid rejectedByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var request = await _timeOffRepository.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new KeyNotFoundException("İzin talebi bulunamadı.");
        }

        if (request.Status != TimeOffStatus.Pending)
        {
            throw new InvalidOperationException("Sadece bekleyen talepler reddedilebilir.");
        }

        request.Status = TimeOffStatus.Rejected;
        request.RejectionReason = reason;
        request.UpdatedAt = DateTime.UtcNow;

        await _timeOffRepository.UpdateAsync(request);

        // Send notification email
        var staff = await _staffRepository.GetByIdAsync(request.StaffId);
        if (staff != null)
        {
            await _emailService.SendTimeOffRejectedAsync(
                staff.Email,
                staff.FullName,
                request.StartDate,
                request.EndDate,
                reason,
                cancellationToken);
        }

        _logger.LogInformation("Time off request {RequestId} rejected", requestId);

        return await MapToTimeOffDto(request);
    }

    public async Task CancelTimeOffRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _timeOffRepository.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new KeyNotFoundException("İzin talebi bulunamadı.");
        }

        if (request.Status == TimeOffStatus.Approved && request.StartDate <= DateTime.Today)
        {
            throw new InvalidOperationException("Başlamış izinler iptal edilemez.");
        }

        request.Status = TimeOffStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _timeOffRepository.UpdateAsync(request);

        _logger.LogInformation("Time off request {RequestId} cancelled", requestId);
    }

    // Helper method
    private async Task<TimeOffRequestDto> MapToTimeOffDto(TimeOffRequest request)
    {
        var staff = await _staffRepository.GetByIdAsync(request.StaffId);
        
        return new TimeOffRequestDto
        {
            Id = request.Id,
            StaffId = request.StaffId,
            StaffName = staff?.FullName ?? "Unknown",
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DaysRequested = request.DaysRequested,
            Reason = request.Reason,
            Status = request.Status,
            ApprovedByUserId = request.ApprovedByUserId,
            ApprovedAt = request.ApprovedAt,
            RejectionReason = request.RejectionReason
        };
    }
}
