using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace RendevumVar.Application.Services;

public class SalonService : ISalonService
{
    private readonly ISalonRepository _salonRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<SalonService> _logger;

    public SalonService(
        ISalonRepository salonRepository,
        IServiceRepository serviceRepository,
        ILogger<SalonService> logger)
    {
        _salonRepository = salonRepository;
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<SalonDto> CreateSalonAsync(CreateSalonDto dto, Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating salon {Name} for tenant {TenantId}", dto.Name, tenantId);

        var salon = new Salon
        {
            TenantId = tenantId,
            Name = dto.Name,
            Description = dto.Description,
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            BusinessHours = dto.BusinessHours,
            IsActive = true
        };

        var created = await _salonRepository.CreateAsync(salon, cancellationToken);
        return MapToDto(created);
    }

    public async Task<SalonDto> UpdateSalonAsync(UpdateSalonDto dto, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var salon = await _salonRepository.GetByIdAsync(dto.Id, cancellationToken);

        if (salon == null)
            throw new InvalidOperationException($"Salon with ID {dto.Id} not found.");

        if (salon.TenantId != tenantId)
            throw new UnauthorizedAccessException("You do not have permission to update this salon.");

        _logger.LogInformation("Updating salon {SalonId} for tenant {TenantId}", dto.Id, tenantId);

        salon.Name = dto.Name;
        salon.Description = dto.Description;
        salon.Phone = dto.Phone;
        salon.Email = dto.Email;
        salon.Website = dto.Website;
        salon.Address = dto.Address;
        salon.City = dto.City;
        salon.State = dto.State;
        salon.PostalCode = dto.PostalCode;
        salon.Latitude = dto.Latitude;
        salon.Longitude = dto.Longitude;
        salon.BusinessHours = dto.BusinessHours;
        salon.IsActive = dto.IsActive;

        await _salonRepository.UpdateAsync(salon, cancellationToken);
        return MapToDto(salon);
    }

    public async Task<SalonDetailsDto?> GetSalonDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var salon = await _salonRepository.GetSalonFullDetailsAsync(id, cancellationToken);

        if (salon == null)
            return null;

        return new SalonDetailsDto
        {
            Id = salon.Id,
            TenantId = salon.TenantId,
            Name = salon.Name,
            Description = salon.Description,
            Phone = salon.Phone,
            Email = salon.Email,
            Website = salon.Website,
            Address = salon.Address,
            City = salon.City,
            State = salon.State,
            PostalCode = salon.PostalCode,
            Latitude = salon.Latitude,
            Longitude = salon.Longitude,
            BusinessHours = salon.BusinessHours,
            AverageRating = salon.AverageRating,
            ReviewCount = salon.ReviewCount,
            IsActive = salon.IsActive,
            CreatedAt = salon.CreatedAt,
            PrimaryImageUrl = salon.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
            Images = salon.Images?.Select(i => new SalonImageDto
            {
                Id = i.Id,
                SalonId = i.SalonId,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList() ?? new(),
            Services = salon.Services?.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price,
                ImageUrl = s.ImageUrl,
                CategoryId = s.CategoryId,
                CategoryName = s.Category?.Name
            }).ToList() ?? new(),
            Staff = salon.Staff?.Select(st => new StaffDto
            {
                Id = st.Id,
                FirstName = st.FirstName,
                LastName = st.LastName,
                FullName = $"{st.FirstName} {st.LastName}",
                Email = st.Email,
                Phone = st.Phone,
                Bio = st.Bio,
                AverageRating = st.AverageRating,
                ProfilePictureUrl = st.PhotoUrl,
                RoleId = st.RoleId,
                RoleName = st.Role?.Name,
                Status = st.Status
            }).ToList() ?? new()
        };
    }

    public async Task<SalonDto?> GetSalonByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var salon = await _salonRepository.GetSalonWithImagesAsync(id, cancellationToken);

        if (salon == null)
            return null;

        return MapToDto(salon);
    }

    public async Task<List<SalonDto>> GetSalonsByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var salons = await _salonRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        return salons.Select(MapToDto).ToList();
    }

    public async Task<SalonSearchResultDto> SearchSalonsAsync(SearchSalonDto dto, CancellationToken cancellationToken = default)
    {
        IEnumerable<Salon> salons;
        int totalCount;

        // If location-based search
        if (dto.Latitude.HasValue && dto.Longitude.HasValue && dto.RadiusKm.HasValue)
        {
            _logger.LogInformation("Searching salons by location: {Lat}, {Lon}, radius: {Radius}km",
                dto.Latitude, dto.Longitude, dto.RadiusKm);

            salons = await _salonRepository.GetSalonsByLocationAsync(
                dto.Latitude.Value,
                dto.Longitude.Value,
                dto.RadiusKm.Value,
                cancellationToken);

            // Apply additional filters
            if (!string.IsNullOrWhiteSpace(dto.SearchTerm))
            {
                var term = dto.SearchTerm.ToLower();
                salons = salons.Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    (s.Description != null && s.Description.ToLower().Contains(term)));
            }

            if (dto.MinRating.HasValue)
            {
                salons = salons.Where(s => s.AverageRating >= dto.MinRating.Value);
            }

            totalCount = salons.Count();
            salons = salons
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize);
        }
        // If service-based search
        else if (dto.ServiceIds != null && dto.ServiceIds.Any())
        {
            _logger.LogInformation("Searching salons by service IDs: {ServiceIds}", string.Join(", ", dto.ServiceIds));

            salons = await _salonRepository.GetSalonsByServiceIdsAsync(dto.ServiceIds, cancellationToken);

            // Apply additional filters
            if (!string.IsNullOrWhiteSpace(dto.SearchTerm))
            {
                var term = dto.SearchTerm.ToLower();
                salons = salons.Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    (s.Description != null && s.Description.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(dto.City))
            {
                salons = salons.Where(s => s.City.Equals(dto.City, StringComparison.OrdinalIgnoreCase));
            }

            if (dto.MinRating.HasValue)
            {
                salons = salons.Where(s => s.AverageRating >= dto.MinRating.Value);
            }

            totalCount = salons.Count();
            salons = salons
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize);
        }
        // Standard search
        else
        {
            _logger.LogInformation("Searching salons with term: {SearchTerm}, city: {City}",
                dto.SearchTerm, dto.City);

            totalCount = await _salonRepository.GetTotalCountAsync(
                dto.SearchTerm,
                dto.City,
                dto.MinRating,
                true,
                cancellationToken);

            salons = await _salonRepository.SearchSalonsAsync(
                dto.SearchTerm,
                dto.City,
                dto.MinRating,
                true,
                dto.PageNumber,
                dto.PageSize,
                cancellationToken);
        }

        return new SalonSearchResultDto
        {
            Salons = salons.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    public async Task DeleteSalonAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var salon = await _salonRepository.GetByIdAsync(id, cancellationToken);

        if (salon == null)
            throw new InvalidOperationException($"Salon with ID {id} not found.");

        if (salon.TenantId != tenantId)
            throw new UnauthorizedAccessException("You do not have permission to delete this salon.");

        _logger.LogInformation("Deleting salon {SalonId} for tenant {TenantId}", id, tenantId);

        await _salonRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<SalonImageDto> UploadSalonImageAsync(Guid salonId, Guid tenantId, string imageUrl, bool isPrimary = false, CancellationToken cancellationToken = default)
    {
        var salon = await _salonRepository.GetSalonWithImagesAsync(salonId, cancellationToken);

        if (salon == null)
            throw new InvalidOperationException($"Salon with ID {salonId} not found.");

        if (salon.TenantId != tenantId)
            throw new UnauthorizedAccessException("You do not have permission to upload images to this salon.");

        _logger.LogInformation("Uploading image to salon {SalonId}", salonId);

        // If setting as primary, unset other primary images
        if (isPrimary && salon.Images != null)
        {
            foreach (var img in salon.Images.Where(i => i.IsPrimary))
            {
                img.IsPrimary = false;
            }
        }

        var maxDisplayOrder = salon.Images?.Any() == true ? salon.Images.Max(i => i.DisplayOrder) : 0;

        var salonImage = new SalonImage
        {
            SalonId = salonId,
            ImageUrl = imageUrl,
            IsPrimary = isPrimary,
            DisplayOrder = maxDisplayOrder + 1
        };

        salon.Images ??= new List<SalonImage>();
        salon.Images.Add(salonImage);

        await _salonRepository.UpdateAsync(salon, cancellationToken);

        return new SalonImageDto
        {
            Id = salonImage.Id,
            SalonId = salonImage.SalonId,
            ImageUrl = salonImage.ImageUrl,
            IsPrimary = salonImage.IsPrimary,
            DisplayOrder = salonImage.DisplayOrder
        };
    }

    public async Task DeleteSalonImageAsync(Guid imageId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var salon = await _salonRepository.GetAllAsync(cancellationToken);
        var salonWithImage = salon.FirstOrDefault(s => s.Images.Any(i => i.Id == imageId));

        if (salonWithImage == null)
            throw new InvalidOperationException($"Image with ID {imageId} not found.");

        if (salonWithImage.TenantId != tenantId)
            throw new UnauthorizedAccessException("You do not have permission to delete this image.");

        _logger.LogInformation("Deleting image {ImageId} from salon {SalonId}", imageId, salonWithImage.Id);

        var image = salonWithImage.Images.First(i => i.Id == imageId);
        salonWithImage.Images.Remove(image);

        await _salonRepository.UpdateAsync(salonWithImage, cancellationToken);
    }

    public async Task UpdateBusinessHoursAsync(UpdateBusinessHoursDto dto, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var salon = await _salonRepository.GetByIdAsync(dto.SalonId, cancellationToken);

        if (salon == null)
            throw new InvalidOperationException($"Salon with ID {dto.SalonId} not found.");

        if (salon.TenantId != tenantId)
            throw new UnauthorizedAccessException("You do not have permission to update this salon's business hours.");

        _logger.LogInformation("Updating business hours for salon {SalonId}", dto.SalonId);

        salon.BusinessHours = dto.BusinessHours;
        await _salonRepository.UpdateAsync(salon, cancellationToken);
    }

    public async Task UpdateAverageRatingAsync(Guid salonId, CancellationToken cancellationToken = default)
    {
        // This will be implemented when reviews are added
        // For now, just a placeholder
        _logger.LogInformation("Updating average rating for salon {SalonId}", salonId);

        var salon = await _salonRepository.GetByIdAsync(salonId, cancellationToken);
        if (salon != null)
        {
            // TODO: Calculate from reviews when review system is implemented
            await _salonRepository.UpdateAsync(salon, cancellationToken);
        }
    }

    private SalonDto MapToDto(Salon salon)
    {
        return new SalonDto
        {
            Id = salon.Id,
            TenantId = salon.TenantId,
            Name = salon.Name,
            Description = salon.Description,
            Phone = salon.Phone,
            Email = salon.Email,
            Website = salon.Website,
            Address = salon.Address,
            City = salon.City,
            State = salon.State,
            PostalCode = salon.PostalCode,
            Latitude = salon.Latitude,
            Longitude = salon.Longitude,
            BusinessHours = salon.BusinessHours,
            AverageRating = salon.AverageRating,
            ReviewCount = salon.ReviewCount,
            IsActive = salon.IsActive,
            CreatedAt = salon.CreatedAt,
            PrimaryImageUrl = salon.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
        };
    }
}
