using RendevumVar.Application.DTOs;

namespace RendevumVar.Application.Services;

public interface IStaffService
{
    // Staff invitation & onboarding
    Task<StaffDto> InviteStaffAsync(Guid tenantId, InviteStaffDto dto, CancellationToken cancellationToken = default);
    Task<StaffDto> AcceptInvitationAsync(AcceptInvitationDto dto, CancellationToken cancellationToken = default);
    Task ResendInvitationAsync(Guid staffId, CancellationToken cancellationToken = default);
    
    // Staff management
    Task<IEnumerable<StaffDto>> GetStaffListAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<StaffDto> GetStaffDetailsAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<StaffDto> UpdateStaffProfileAsync(Guid staffId, UpdateStaffProfileDto dto, CancellationToken cancellationToken = default);
    Task DeactivateStaffAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task ReactivateStaffAsync(Guid staffId, CancellationToken cancellationToken = default);
    
    // Role management
    Task AssignRoleAsync(Guid staffId, Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleDto>> GetRolesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateRoleAsync(Guid tenantId, CreateRoleDto dto, CancellationToken cancellationToken = default);
}
