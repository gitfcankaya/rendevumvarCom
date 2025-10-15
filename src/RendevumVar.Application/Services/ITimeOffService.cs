using RendevumVar.Application.DTOs;

namespace RendevumVar.Application.Services;

public interface ITimeOffService
{
    // Time off request management
    Task<TimeOffRequestDto> RequestTimeOffAsync(Guid staffId, CreateTimeOffRequestDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeOffRequestDto>> GetStaffTimeOffAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeOffRequestDto>> GetPendingRequestsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    // Approval workflow
    Task<TimeOffRequestDto> ApproveTimeOffRequestAsync(Guid requestId, Guid approvedByUserId, CancellationToken cancellationToken = default);
    Task<TimeOffRequestDto> RejectTimeOffRequestAsync(Guid requestId, Guid rejectedByUserId, string reason, CancellationToken cancellationToken = default);
    Task CancelTimeOffRequestAsync(Guid requestId, CancellationToken cancellationToken = default);
}
