using RendevumVar.Application.DTOs;

namespace RendevumVar.Application.Interfaces;

public interface ISalonService
{
    Task<SalonDto> CreateSalonAsync(CreateSalonDto dto, Guid tenantId, CancellationToken cancellationToken = default);
    Task<SalonDto> UpdateSalonAsync(UpdateSalonDto dto, Guid tenantId, CancellationToken cancellationToken = default);
    Task<SalonDetailsDto?> GetSalonDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SalonDto?> GetSalonByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SalonDto>> GetSalonsByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<SalonSearchResultDto> SearchSalonsAsync(SearchSalonDto dto, CancellationToken cancellationToken = default);
    Task DeleteSalonAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<SalonImageDto> UploadSalonImageAsync(Guid salonId, Guid tenantId, string imageUrl, bool isPrimary = false, CancellationToken cancellationToken = default);
    Task DeleteSalonImageAsync(Guid imageId, Guid tenantId, CancellationToken cancellationToken = default);
    Task UpdateBusinessHoursAsync(UpdateBusinessHoursDto dto, Guid tenantId, CancellationToken cancellationToken = default);
    Task UpdateAverageRatingAsync(Guid salonId, CancellationToken cancellationToken = default);
}
