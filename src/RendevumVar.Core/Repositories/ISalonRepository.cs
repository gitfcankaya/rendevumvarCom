using RendevumVar.Core.Entities;

namespace RendevumVar.Core.Repositories;

public interface ISalonRepository
{
    Task<Salon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Salon?> GetSalonWithImagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Salon?> GetSalonWithServicesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Salon?> GetSalonWithStaffAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Salon?> GetSalonFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Salon>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Salon>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Salon>> SearchSalonsAsync(
        string? searchTerm = null,
        string? city = null,
        decimal? minRating = null,
        bool? isActive = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        string? city = null,
        decimal? minRating = null,
        bool? isActive = true,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Salon>> GetSalonsByLocationAsync(
        decimal latitude,
        decimal longitude,
        double radiusKm,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Salon>> GetSalonsByServiceIdsAsync(
        IEnumerable<Guid> serviceIds,
        CancellationToken cancellationToken = default);
    Task<Salon> CreateAsync(Salon salon, CancellationToken cancellationToken = default);
    Task UpdateAsync(Salon salon, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
