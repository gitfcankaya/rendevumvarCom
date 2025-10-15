using Microsoft.Extensions.Logging;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;

namespace RendevumVar.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISalonRepository _salonRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IServiceRepository serviceRepository,
        IStaffRepository staffRepository,
        ISalonRepository salonRepository,
        INotificationService notificationService,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _serviceRepository = serviceRepository;
        _staffRepository = staffRepository;
        _salonRepository = salonRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(
        CreateAppointmentDto dto,
        Guid customerId,
        Guid tenantId)
    {
        // Validate service exists
        var service = await _serviceRepository.GetByIdAsync(dto.ServiceId);
        if (service == null || service.TenantId != tenantId)
            throw new InvalidOperationException("Service not found");

        // Validate staff exists and belongs to salon
        var staff = await _staffRepository.GetByIdAsync(dto.StaffId);
        if (staff == null || staff.TenantId != tenantId || staff.SalonId != dto.SalonId)
            throw new InvalidOperationException("Staff not found or does not work at this salon");

        // Validate salon exists
        var salon = await _salonRepository.GetByIdAsync(dto.SalonId);
        if (salon == null || salon.TenantId != tenantId)
            throw new InvalidOperationException("Salon not found");

        // Calculate end time
        var endTime = dto.StartTime.AddMinutes(service.DurationMinutes);

        // Check for conflicts
        var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            dto.StaffId,
            dto.StartTime,
            endTime);

        if (hasConflict)
            throw new InvalidOperationException("This time slot is not available");

        // Validate appointment time is in the future
        if (dto.StartTime <= DateTime.UtcNow)
            throw new InvalidOperationException("Appointment time must be in the future");

        // Create appointment
        var appointment = new Appointment
        {
            TenantId = tenantId,
            SalonId = dto.SalonId,
            CustomerId = customerId,
            StaffId = dto.StaffId,
            ServiceId = dto.ServiceId,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            Notes = dto.Notes,
            CustomerNotes = dto.CustomerNotes,
            TotalPrice = service.Price
        };

        await _appointmentRepository.AddAsync(appointment);

        _logger.LogInformation("Appointment created: {AppointmentId} for customer {CustomerId}",
            appointment.Id, customerId);

        // Send confirmation email
        try
        {
            await _notificationService.SendAppointmentConfirmationAsync(appointment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for appointment {AppointmentId}", appointment.Id);
            // Don't fail the appointment creation if email fails
        }

        return MapToDto(appointment, service, staff, salon);
    }

    public async Task<AppointmentDetailsDto?> GetAppointmentDetailsAsync(Guid id, Guid tenantId)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);

        if (appointment == null || appointment.TenantId != tenantId)
            return null;

        return new AppointmentDetailsDto
        {
            Id = appointment.Id,
            TenantId = appointment.TenantId,
            SalonId = appointment.SalonId,
            SalonName = appointment.Salon.Name,
            CustomerId = appointment.CustomerId,
            CustomerName = $"{appointment.Customer.FirstName} {appointment.Customer.LastName}",
            CustomerEmail = appointment.Customer.Email,
            CustomerPhone = appointment.Customer.Phone,
            StaffId = appointment.StaffId,
            StaffName = $"{appointment.Staff.FirstName} {appointment.Staff.LastName}",
            ServiceId = appointment.ServiceId,
            ServiceName = appointment.Service.Name,
            ServiceDuration = appointment.Service.DurationMinutes,
            ServicePrice = appointment.Service.Price,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CustomerNotes = appointment.CustomerNotes,
            CancellationReason = appointment.CancellationReason,
            CancelledAt = appointment.CancelledAt,
            TotalPrice = appointment.TotalPrice,
            DepositPaid = appointment.DepositPaid,
            ReminderSent = appointment.ReminderSent,
            CreatedAt = appointment.CreatedAt,
            Salon = new SalonDto
            {
                Id = appointment.Salon.Id,
                TenantId = appointment.Salon.TenantId,
                Name = appointment.Salon.Name,
                Address = appointment.Salon.Address,
                City = appointment.Salon.City,
                Phone = appointment.Salon.Phone,
                Email = appointment.Salon.Email,
                State = appointment.Salon.State,
                PostalCode = appointment.Salon.PostalCode,
                Latitude = appointment.Salon.Latitude,
                Longitude = appointment.Salon.Longitude,
                BusinessHours = appointment.Salon.BusinessHours,
                IsActive = appointment.Salon.IsActive,
                AverageRating = appointment.Salon.AverageRating,
                ReviewCount = appointment.Salon.ReviewCount,
                CreatedAt = appointment.Salon.CreatedAt
            },
            Service = new ServiceDto
            {
                Id = appointment.Service.Id,
                Name = appointment.Service.Name,
                Description = appointment.Service.Description,
                DurationMinutes = appointment.Service.DurationMinutes,
                Price = appointment.Service.Price,
                ImageUrl = appointment.Service.ImageUrl,
                CategoryId = appointment.Service.CategoryId,
                CategoryName = appointment.Service.Category?.Name,
                IsActive = appointment.Service.IsActive
            },
            Staff = new StaffDto
            {
                Id = appointment.Staff.Id,
                TenantId = appointment.Staff.TenantId,
                SalonId = appointment.Staff.SalonId,
                FirstName = appointment.Staff.FirstName,
                LastName = appointment.Staff.LastName,
                FullName = $"{appointment.Staff.FirstName} {appointment.Staff.LastName}",
                Email = appointment.Staff.Email,
                Phone = appointment.Staff.Phone,
                Bio = appointment.Staff.Bio,
                PhotoUrl = appointment.Staff.PhotoUrl,
                ProfilePictureUrl = appointment.Staff.PhotoUrl,
                AverageRating = appointment.Staff.AverageRating,
                Status = appointment.Staff.Status,
                InvitationStatus = appointment.Staff.InvitationStatus,
                RoleId = appointment.Staff.RoleId,
                RoleName = appointment.Staff.Role?.Name
            },
            Payments = appointment.Payments?.Select(p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Method = p.Method,
                Status = p.Status
            }).ToList() ?? new()
        };
    }

    public async Task<AppointmentDto> UpdateAppointmentAsync(UpdateAppointmentDto dto, Guid tenantId)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(dto.Id);

        if (appointment == null || appointment.TenantId != tenantId)
            throw new InvalidOperationException("Appointment not found");

        if (appointment.Status == AppointmentStatus.Completed ||
            appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot update completed or cancelled appointments");

        // If changing staff, validate the new staff
        if (dto.StaffId.HasValue && dto.StaffId.Value != appointment.StaffId)
        {
            var staff = await _staffRepository.GetByIdAsync(dto.StaffId.Value);
            if (staff == null || staff.TenantId != tenantId || staff.SalonId != appointment.SalonId)
                throw new InvalidOperationException("Invalid staff selection");

            appointment.StaffId = dto.StaffId.Value;
        }

        // If changing time, check for conflicts
        if (dto.StartTime != appointment.StartTime)
        {
            var endTime = dto.StartTime.AddMinutes(appointment.Service.DurationMinutes);

            var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                appointment.StaffId,
                dto.StartTime,
                endTime,
                appointment.Id);

            if (hasConflict)
                throw new InvalidOperationException("This time slot is not available");

            appointment.StartTime = dto.StartTime;
            appointment.EndTime = endTime;
        }

        if (dto.Notes != null)
            appointment.Notes = dto.Notes;

        await _appointmentRepository.UpdateAsync(appointment);

        _logger.LogInformation("Appointment updated: {AppointmentId}", appointment.Id);

        return MapToDto(appointment, appointment.Service, appointment.Staff, appointment.Salon);
    }

    public async Task<AppointmentDto> UpdateAppointmentStatusAsync(
        Guid id,
        UpdateAppointmentStatusDto dto,
        Guid tenantId)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);

        if (appointment == null || appointment.TenantId != tenantId)
            throw new InvalidOperationException("Appointment not found");

        // Validate status transition
        if (!IsValidStatusTransition(appointment.Status, dto.Status))
            throw new InvalidOperationException($"Cannot change status from {appointment.Status} to {dto.Status}");

        appointment.Status = dto.Status;

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            appointment.CancellationReason = dto.CancellationReason;
            appointment.CancelledAt = DateTime.UtcNow;
        }

        await _appointmentRepository.UpdateAsync(appointment);

        _logger.LogInformation("Appointment status updated: {AppointmentId} to {Status}", id, dto.Status);

        // Send status update notification
        try
        {
            await _notificationService.SendStatusUpdateNotificationAsync(id, dto.Status.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send status update notification for appointment {AppointmentId}", id);
        }

        return MapToDto(appointment, appointment.Service, appointment.Staff, appointment.Salon);
    }

    public async Task CancelAppointmentAsync(Guid id, string? cancellationReason, Guid tenantId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);

        if (appointment == null || appointment.TenantId != tenantId)
            throw new InvalidOperationException("Appointment not found");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed appointments");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = cancellationReason;
        appointment.CancelledAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);

        _logger.LogInformation("Appointment cancelled: {AppointmentId}", id);

        // Send cancellation notification
        try
        {
            await _notificationService.SendCancellationNotificationAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send cancellation notification for appointment {AppointmentId}", id);
        }
    }

    public async Task<AppointmentDto> RescheduleAppointmentAsync(
        Guid id,
        DateTime newStartTime,
        Guid? newStaffId,
        Guid tenantId)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);

        if (appointment == null || appointment.TenantId != tenantId)
            throw new InvalidOperationException("Appointment not found");

        if (appointment.Status == AppointmentStatus.Completed ||
            appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule completed or cancelled appointments");

        var staffId = newStaffId ?? appointment.StaffId;
        var oldStartTime = appointment.StartTime; // Store old time for notification
        var endTime = newStartTime.AddMinutes(appointment.Service.DurationMinutes);

        // Check for conflicts
        var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            staffId,
            newStartTime,
            endTime,
            appointment.Id);

        if (hasConflict)
            throw new InvalidOperationException("This time slot is not available");

        if (newStaffId.HasValue && newStaffId.Value != appointment.StaffId)
        {
            var staff = await _staffRepository.GetByIdAsync(newStaffId.Value);
            if (staff == null || staff.TenantId != tenantId || staff.SalonId != appointment.SalonId)
                throw new InvalidOperationException("Invalid staff selection");

            appointment.StaffId = newStaffId.Value;
        }

        appointment.StartTime = newStartTime;
        appointment.EndTime = endTime;
        appointment.Status = AppointmentStatus.Pending; // Reset to pending after rescheduling

        await _appointmentRepository.UpdateAsync(appointment);

        // Reload with details
        appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);

        _logger.LogInformation("Appointment rescheduled: {AppointmentId}", id);

        // Send reschedule notification
        try
        {
            await _notificationService.SendRescheduleNotificationAsync(id, oldStartTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reschedule notification for appointment {AppointmentId}", id);
        }

        return MapToDto(appointment!, appointment!.Service, appointment.Staff, appointment.Salon);
    }

    public async Task<IEnumerable<AppointmentDto>> GetCustomerAppointmentsAsync(
        Guid customerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        AppointmentStatus? status = null)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByCustomerAsync(
            customerId,
            startDate,
            endDate,
            status);

        return appointments.Select(a => MapToDto(a, a.Service, a.Staff, a.Salon)).ToList();
    }

    public async Task<IEnumerable<AppointmentDto>> GetStaffAppointmentsAsync(
        Guid staffId,
        Guid tenantId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null || staff.TenantId != tenantId)
            throw new InvalidOperationException("Staff not found");

        var appointments = await _appointmentRepository.GetAppointmentsByStaffAsync(
            staffId,
            startDate,
            endDate);

        return appointments.Select(a => MapToDto(a, a.Service, a.Staff, a.Salon)).ToList();
    }

    public async Task<IEnumerable<AppointmentDto>> GetSalonAppointmentsAsync(
        Guid salonId,
        Guid tenantId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.TenantId != tenantId)
            throw new InvalidOperationException("Salon not found");

        var appointments = await _appointmentRepository.GetAppointmentsBySalonAsync(
            salonId,
            startDate,
            endDate);

        return appointments.Select(a => MapToDto(a, a.Service, a.Staff, a.Salon)).ToList();
    }

    public async Task<IEnumerable<AvailableTimeSlotDto>> GetAvailableTimeSlotsAsync(
        Guid staffId,
        DateTime date,
        int serviceDurationMinutes)
    {
        var slots = await _appointmentRepository.GetAvailableTimeSlotsAsync(
            staffId,
            date,
            serviceDurationMinutes);

        return slots.Select(s => new AvailableTimeSlotDto
        {
            Date = s.Date,
            StartTime = s.TimeOfDay,
            EndTime = s.AddMinutes(serviceDurationMinutes).TimeOfDay,
            DurationMinutes = serviceDurationMinutes
        }).ToList();
    }

    public async Task<bool> HasConflictingAppointmentAsync(
        Guid staffId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null)
    {
        return await _appointmentRepository.HasConflictingAppointmentAsync(
            staffId,
            startTime,
            endTime,
            excludeAppointmentId);
    }

    private AppointmentDto MapToDto(Appointment appointment, Service service, Staff staff, Salon salon)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            TenantId = appointment.TenantId,
            SalonId = appointment.SalonId,
            SalonName = salon.Name,
            CustomerId = appointment.CustomerId,
            CustomerName = $"{appointment.Customer?.FirstName} {appointment.Customer?.LastName}",
            CustomerEmail = appointment.Customer?.Email ?? "",
            CustomerPhone = appointment.Customer?.Phone,
            StaffId = appointment.StaffId,
            StaffName = $"{staff.FirstName} {staff.LastName}",
            ServiceId = appointment.ServiceId,
            ServiceName = service.Name,
            ServiceDuration = service.DurationMinutes,
            ServicePrice = service.Price,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CustomerNotes = appointment.CustomerNotes,
            CancellationReason = appointment.CancellationReason,
            CancelledAt = appointment.CancelledAt,
            TotalPrice = appointment.TotalPrice,
            DepositPaid = appointment.DepositPaid,
            ReminderSent = appointment.ReminderSent,
            CreatedAt = appointment.CreatedAt
        };
    }

    private bool IsValidStatusTransition(AppointmentStatus currentStatus, AppointmentStatus newStatus)
    {
        // Define valid status transitions
        return (currentStatus, newStatus) switch
        {
            (AppointmentStatus.Pending, AppointmentStatus.Confirmed) => true,
            (AppointmentStatus.Pending, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.CheckedIn) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.NoShow) => true,
            (AppointmentStatus.CheckedIn, AppointmentStatus.InProgress) => true,
            (AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Completed) => true,
            _ => false
        };
    }
}
