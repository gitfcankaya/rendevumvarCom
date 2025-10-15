using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RendevumVar.Application.DTOs;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;

namespace RendevumVar.Application.Services;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<StaffService> _logger;

    public StaffService(
        IStaffRepository staffRepository,
        IRoleRepository roleRepository,
        IEmailService emailService,
        ILogger<StaffService> logger)
    {
        _staffRepository = staffRepository;
        _roleRepository = roleRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<StaffDto> InviteStaffAsync(Guid tenantId, InviteStaffDto dto, CancellationToken cancellationToken = default)
    {
        // Check if email is unique
        if (!await _staffRepository.IsEmailUniqueAsync(dto.Email, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Bu email adresi zaten kullanılıyor.");
        }

        // Generate invitation token
        var invitationToken = GenerateInvitationToken();
        var invitationExpiresAt = DateTime.UtcNow.AddDays(7); // 7 days expiry

        // Create staff record
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SalonId = dto.SalonId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            RoleId = dto.RoleId,
            Specialties = dto.Specialization,
            Status = StaffStatus.Invited,
            InvitationStatus = InvitationStatus.Pending,
            InvitationToken = invitationToken,
            InvitationSentAt = DateTime.UtcNow,
            InvitationExpiresAt = invitationExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await _staffRepository.AddAsync(staff);

        // Send invitation email
        var invitationLink = $"https://yourapp.com/accept-invitation?token={invitationToken}";
        await _emailService.SendStaffInvitationAsync(
            staff.Email,
            staff.FullName,
            invitationLink,
            cancellationToken);

        _logger.LogInformation("Staff invitation sent to {Email}", staff.Email);

        return MapToStaffDto(staff);
    }

    public async Task<StaffDto> AcceptInvitationAsync(AcceptInvitationDto dto, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByInvitationTokenAsync(dto.Token, cancellationToken);
        if (staff == null)
        {
            throw new InvalidOperationException("Geçersiz veya süresi dolmuş davet bağlantısı.");
        }

        // Update staff record
        staff.InvitationStatus = InvitationStatus.Accepted;
        staff.InvitationAcceptedAt = DateTime.UtcNow;
        staff.Status = StaffStatus.Active;
        staff.Phone = dto.Phone ?? staff.Phone;
        staff.Bio = dto.Bio;
        staff.HireDate = DateTime.UtcNow;
        staff.UpdatedAt = DateTime.UtcNow;
        staff.UpdatedBy = staff.Email;

        // TODO: Create user account with password (requires IAuthService integration)
        
        await _staffRepository.UpdateAsync(staff);

        _logger.LogInformation("Staff {Email} accepted invitation", staff.Email);

        return MapToStaffDto(staff);
    }

    public async Task ResendInvitationAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            throw new KeyNotFoundException("Personel bulunamadı.");
        }

        if (staff.InvitationStatus != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Sadece bekleyen davetler yeniden gönderilebilir.");
        }

        // Generate new token
        staff.InvitationToken = GenerateInvitationToken();
        staff.InvitationSentAt = DateTime.UtcNow;
        staff.InvitationExpiresAt = DateTime.UtcNow.AddDays(7);
        staff.UpdatedAt = DateTime.UtcNow;

        await _staffRepository.UpdateAsync(staff);

        // Send invitation email
        var invitationLink = $"https://yourapp.com/accept-invitation?token={staff.InvitationToken}";
        await _emailService.SendStaffInvitationAsync(
            staff.Email,
            staff.FullName,
            invitationLink,
            cancellationToken);

        _logger.LogInformation("Invitation resent to {Email}", staff.Email);
    }

    public async Task<IEnumerable<StaffDto>> GetStaffListAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var staffList = await _staffRepository.GetAllAsync();
        var tenantStaff = staffList.Where(s => s.TenantId == tenantId);

        var dtos = new List<StaffDto>();
        foreach (var staff in tenantStaff)
        {
            dtos.Add(MapToStaffDto(staff));
        }

        return dtos;
    }

    public async Task<StaffDto> GetStaffDetailsAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetWithRoleAsync(staffId, cancellationToken);
        if (staff == null)
        {
            throw new KeyNotFoundException("Personel bulunamadı.");
        }

        return MapToStaffDto(staff);
    }

    public async Task<StaffDto> UpdateStaffProfileAsync(Guid staffId, UpdateStaffProfileDto dto, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            throw new KeyNotFoundException("Personel bulunamadı.");
        }

        // Update fields
        if (!string.IsNullOrEmpty(dto.FirstName)) staff.FirstName = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName)) staff.LastName = dto.LastName;
        if (dto.Phone != null) staff.Phone = dto.Phone;
        if (dto.Bio != null) staff.Bio = dto.Bio;
        if (dto.Specialization != null) staff.Specialties = dto.Specialization;
        if (dto.PhotoUrl != null) staff.PhotoUrl = dto.PhotoUrl;
        if (dto.HourlyRate.HasValue) staff.HourlyRate = dto.HourlyRate;
        if (dto.CommissionRate.HasValue) staff.CommissionRate = dto.CommissionRate;

        staff.UpdatedAt = DateTime.UtcNow;

        await _staffRepository.UpdateAsync(staff);

        _logger.LogInformation("Staff profile updated: {StaffId}", staffId);

        return MapToStaffDto(staff);
    }

    public async Task DeactivateStaffAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            throw new KeyNotFoundException("Personel bulunamadı.");
        }

        staff.Status = StaffStatus.Inactive;
        staff.IsActive = false;
        staff.UpdatedAt = DateTime.UtcNow;

        await _staffRepository.UpdateAsync(staff);

        _logger.LogInformation("Staff deactivated: {StaffId}", staffId);
    }

    public async Task ReactivateStaffAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            throw new KeyNotFoundException("Personel bulunamadı.");
        }

        staff.Status = StaffStatus.Active;
        staff.IsActive = true;
        staff.UpdatedAt = DateTime.UtcNow;

        await _staffRepository.UpdateAsync(staff);

        _logger.LogInformation("Staff reactivated: {StaffId}", staffId);
    }

    public async Task AssignRoleAsync(Guid staffId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            throw new KeyNotFoundException("Personel bulunamadı.");
        }

        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            throw new KeyNotFoundException("Rol bulunamadı.");
        }

        staff.RoleId = roleId;
        staff.UpdatedAt = DateTime.UtcNow;

        await _staffRepository.UpdateAsync(staff);

        _logger.LogInformation("Role {RoleId} assigned to staff {StaffId}", roleId, staffId);
    }

    public async Task<IEnumerable<RoleDto>> GetRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetTenantRolesAsync(tenantId, cancellationToken);
        return roles.Select(MapToRoleDto);
    }

    public async Task<RoleDto> CreateRoleAsync(Guid tenantId, CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        // Check if name is unique
        if (!await _roleRepository.IsNameUniqueAsync(tenantId, dto.Name, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Bu rol adı zaten kullanılıyor.");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name,
            Description = dto.Description,
            Permissions = JsonSerializer.Serialize(dto.Permissions),
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await _roleRepository.AddAsync(role);

        _logger.LogInformation("Role created: {RoleName}", role.Name);

        return MapToRoleDto(role);
    }

    // Helper methods
    private string GenerateInvitationToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    private StaffDto MapToStaffDto(Staff staff)
    {
        return new StaffDto
        {
            Id = staff.Id,
            TenantId = staff.TenantId,
            SalonId = staff.SalonId,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            FullName = staff.FullName,
            Email = staff.Email,
            Phone = staff.Phone,
            Bio = staff.Bio,
            Specialization = staff.Specialties,
            PhotoUrl = staff.PhotoUrl,
            Status = staff.Status,
            InvitationStatus = staff.InvitationStatus,
            HireDate = staff.HireDate,
            HourlyRate = staff.HourlyRate,
            CommissionRate = staff.CommissionRate,
            Role = staff.Role != null ? MapToRoleDto(staff.Role) : null,
            CreatedAt = staff.CreatedAt
        };
    }

    private RoleDto MapToRoleDto(Role role)
    {
        var permissions = new List<string>();
        try
        {
            permissions = JsonSerializer.Deserialize<List<string>>(role.Permissions) ?? new List<string>();
        }
        catch
        {
            // Ignore deserialization errors
        }

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = permissions,
            IsSystemRole = role.IsSystemRole
        };
    }
}
