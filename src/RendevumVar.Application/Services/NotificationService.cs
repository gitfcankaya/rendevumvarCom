using Microsoft.Extensions.Logging;
using RendevumVar.Application.Interfaces;
using RendevumVar.Application.Services;
using RendevumVar.Core.Repositories;
using System.Text;

namespace RendevumVar.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IAppointmentRepository appointmentRepository,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendAppointmentConfirmationAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(appointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for confirmation email", appointmentId);
                return;
            }

            var subject = "Randevu Onayı - RendevumVar";
            var body = BuildConfirmationEmail(appointment);

            await SendEmailAsync(appointment.Customer.Email, subject, body);

            _logger.LogInformation("Confirmation email sent for appointment {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending confirmation email for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task SendAppointmentReminderAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(appointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for reminder email", appointmentId);
                return;
            }

            if (appointment.ReminderSent)
            {
                _logger.LogInformation("Reminder already sent for appointment {AppointmentId}", appointmentId);
                return;
            }

            var subject = "Randevu Hatırlatması - RendevumVar";
            var body = BuildReminderEmail(appointment);

            await SendEmailAsync(appointment.Customer.Email, subject, body);

            // Mark reminder as sent
            appointment.ReminderSent = true;
            await _appointmentRepository.UpdateAsync(appointment);

            _logger.LogInformation("Reminder email sent for appointment {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reminder email for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task SendCancellationNotificationAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(appointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for cancellation email", appointmentId);
                return;
            }

            var subject = "Randevu İptali - RendevumVar";
            var body = BuildCancellationEmail(appointment);

            // Send to customer
            await SendEmailAsync(appointment.Customer.Email, subject, body);

            // Send notification to salon (if salon has email)
            if (!string.IsNullOrEmpty(appointment.Salon.Email))
            {
                var salonBody = BuildCancellationEmailForSalon(appointment);
                await SendEmailAsync(appointment.Salon.Email, subject, salonBody);
            }

            _logger.LogInformation("Cancellation email sent for appointment {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending cancellation email for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task SendRescheduleNotificationAsync(Guid appointmentId, DateTime oldStartTime, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(appointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for reschedule email", appointmentId);
                return;
            }

            var subject = "Randevu Değişikliği - RendevumVar";
            var body = BuildRescheduleEmail(appointment, oldStartTime);

            await SendEmailAsync(appointment.Customer.Email, subject, body);

            // Send notification to salon
            if (!string.IsNullOrEmpty(appointment.Salon.Email))
            {
                var salonBody = BuildRescheduleEmailForSalon(appointment, oldStartTime);
                await SendEmailAsync(appointment.Salon.Email, subject, salonBody);
            }

            _logger.LogInformation("Reschedule email sent for appointment {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reschedule email for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task SendStatusUpdateNotificationAsync(Guid appointmentId, string newStatus, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(appointmentId, cancellationToken);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for status update email", appointmentId);
                return;
            }

            var subject = $"Randevu Durumu Güncellendi - RendevumVar";
            var body = BuildStatusUpdateEmail(appointment, newStatus);

            await SendEmailAsync(appointment.Customer.Email, subject, body);

            _logger.LogInformation("Status update email sent for appointment {AppointmentId}, new status: {Status}", appointmentId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending status update email for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // For now, we'll use a simple email sending approach
        // In production, you would use the IEmailService with proper templates
        _logger.LogInformation("Email would be sent to {Email} with subject: {Subject}", toEmail, subject);

        // TODO: Integrate with actual email service
        // await _emailService.SendAsync(toEmail, subject, body);

        await Task.CompletedTask;
    }

    private string BuildConfirmationEmail(Core.Entities.Appointment appointment)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Sayın {appointment.Customer.FirstName} {appointment.Customer.LastName},");
        sb.AppendLine();
        sb.AppendLine("Randevunuz başarıyla oluşturulmuştur.");
        sb.AppendLine();
        sb.AppendLine("Randevu Detayları:");
        sb.AppendLine($"Salon: {appointment.Salon.Name}");
        sb.AppendLine($"Adres: {appointment.Salon.Address}, {appointment.Salon.City}");
        sb.AppendLine($"Hizmet: {appointment.Service.Name}");
        sb.AppendLine($"Personel: {appointment.Staff.FirstName} {appointment.Staff.LastName}");
        sb.AppendLine($"Tarih: {appointment.StartTime:dd MMMM yyyy}");
        sb.AppendLine($"Saat: {appointment.StartTime:HH:mm} - {appointment.EndTime:HH:mm}");
        sb.AppendLine($"Ücret: {appointment.TotalPrice:C}");
        sb.AppendLine();
        sb.AppendLine("Randevunuzu iptal etmek veya değiştirmek için hesabınıza giriş yapabilirsiniz.");
        sb.AppendLine();
        sb.AppendLine("İyi günler dileriz,");
        sb.AppendLine("RendevumVar Ekibi");

        return sb.ToString();
    }

    private string BuildReminderEmail(Core.Entities.Appointment appointment)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Sayın {appointment.Customer.FirstName} {appointment.Customer.LastName},");
        sb.AppendLine();
        sb.AppendLine($"Yarın saat {appointment.StartTime:HH:mm}'de randevunuz bulunmaktadır.");
        sb.AppendLine();
        sb.AppendLine("Randevu Detayları:");
        sb.AppendLine($"Salon: {appointment.Salon.Name}");
        sb.AppendLine($"Adres: {appointment.Salon.Address}, {appointment.Salon.City}");
        sb.AppendLine($"Hizmet: {appointment.Service.Name}");
        sb.AppendLine($"Personel: {appointment.Staff.FirstName} {appointment.Staff.LastName}");
        sb.AppendLine($"Tarih: {appointment.StartTime:dd MMMM yyyy}");
        sb.AppendLine($"Saat: {appointment.StartTime:HH:mm} - {appointment.EndTime:HH:mm}");
        sb.AppendLine();
        sb.AppendLine("Randevunuzu iptal etmeniz gerekiyorsa lütfen en kısa sürede bildirin.");
        sb.AppendLine();
        sb.AppendLine("İyi günler dileriz,");
        sb.AppendLine("RendevumVar Ekibi");

        return sb.ToString();
    }

    private string BuildCancellationEmail(Core.Entities.Appointment appointment)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Sayın {appointment.Customer.FirstName} {appointment.Customer.LastName},");
        sb.AppendLine();
        sb.AppendLine("Randevunuz iptal edilmiştir.");
        sb.AppendLine();
        sb.AppendLine("İptal Edilen Randevu Detayları:");
        sb.AppendLine($"Salon: {appointment.Salon.Name}");
        sb.AppendLine($"Hizmet: {appointment.Service.Name}");
        sb.AppendLine($"Personel: {appointment.Staff.FirstName} {appointment.Staff.LastName}");
        sb.AppendLine($"Tarih: {appointment.StartTime:dd MMMM yyyy}");
        sb.AppendLine($"Saat: {appointment.StartTime:HH:mm} - {appointment.EndTime:HH:mm}");

        if (!string.IsNullOrEmpty(appointment.CancellationReason))
        {
            sb.AppendLine();
            sb.AppendLine($"İptal Nedeni: {appointment.CancellationReason}");
        }

        sb.AppendLine();
        sb.AppendLine("Yeni bir randevu oluşturmak için web sitemizi ziyaret edebilirsiniz.");
        sb.AppendLine();
        sb.AppendLine("İyi günler dileriz,");
        sb.AppendLine("RendevumVar Ekibi");

        return sb.ToString();
    }

    private string BuildCancellationEmailForSalon(Core.Entities.Appointment appointment)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Merhaba {appointment.Salon.Name},");
        sb.AppendLine();
        sb.AppendLine("Bir randevu iptal edildi.");
        sb.AppendLine();
        sb.AppendLine("İptal Edilen Randevu Detayları:");
        sb.AppendLine($"Müşteri: {appointment.Customer.FirstName} {appointment.Customer.LastName}");
        sb.AppendLine($"Hizmet: {appointment.Service.Name}");
        sb.AppendLine($"Personel: {appointment.Staff.FirstName} {appointment.Staff.LastName}");
        sb.AppendLine($"Tarih: {appointment.StartTime:dd MMMM yyyy}");
        sb.AppendLine($"Saat: {appointment.StartTime:HH:mm} - {appointment.EndTime:HH:mm}");

        if (!string.IsNullOrEmpty(appointment.CancellationReason))
        {
            sb.AppendLine();
            sb.AppendLine($"İptal Nedeni: {appointment.CancellationReason}");
        }

        sb.AppendLine();
        sb.AppendLine("RendevumVar Sistemi");

        return sb.ToString();
    }

    private string BuildRescheduleEmail(Core.Entities.Appointment appointment, DateTime oldStartTime)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Sayın {appointment.Customer.FirstName} {appointment.Customer.LastName},");
        sb.AppendLine();
        sb.AppendLine("Randevunuz yeni bir tarihe taşınmıştır.");
        sb.AppendLine();
        sb.AppendLine("Eski Randevu:");
        sb.AppendLine($"Tarih: {oldStartTime:dd MMMM yyyy}");
        sb.AppendLine($"Saat: {oldStartTime:HH:mm}");
        sb.AppendLine();
        sb.AppendLine("Yeni Randevu:");
        sb.AppendLine($"Salon: {appointment.Salon.Name}");
        sb.AppendLine($"Adres: {appointment.Salon.Address}, {appointment.Salon.City}");
        sb.AppendLine($"Hizmet: {appointment.Service.Name}");
        sb.AppendLine($"Personel: {appointment.Staff.FirstName} {appointment.Staff.LastName}");
        sb.AppendLine($"Tarih: {appointment.StartTime:dd MMMM yyyy}");
        sb.AppendLine($"Saat: {appointment.StartTime:HH:mm} - {appointment.EndTime:HH:mm}");
        sb.AppendLine();
        sb.AppendLine("İyi günler dileriz,");
        sb.AppendLine("RendevumVar Ekibi");

        return sb.ToString();
    }

    private string BuildRescheduleEmailForSalon(Core.Entities.Appointment appointment, DateTime oldStartTime)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Merhaba {appointment.Salon.Name},");
        sb.AppendLine();
        sb.AppendLine("Bir randevu yeniden planlandı.");
        sb.AppendLine();
        sb.AppendLine($"Müşteri: {appointment.Customer.FirstName} {appointment.Customer.LastName}");
        sb.AppendLine($"Hizmet: {appointment.Service.Name}");
        sb.AppendLine($"Personel: {appointment.Staff.FirstName} {appointment.Staff.LastName}");
        sb.AppendLine();
        sb.AppendLine($"Eski Tarih: {oldStartTime:dd MMMM yyyy}, {oldStartTime:HH:mm}");
        sb.AppendLine($"Yeni Tarih: {appointment.StartTime:dd MMMM yyyy}, {appointment.StartTime:HH:mm}");
        sb.AppendLine();
        sb.AppendLine("RendevumVar Sistemi");

        return sb.ToString();
    }

    private string BuildStatusUpdateEmail(Core.Entities.Appointment appointment, string newStatus)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Sayın {appointment.Customer.FirstName} {appointment.Customer.LastName},");
        sb.AppendLine();
        sb.AppendLine($"Randevunuzun durumu '{newStatus}' olarak güncellendi.");
        sb.AppendLine();
        sb.AppendLine("Randevu Detayları:");
        sb.AppendLine($"Salon: {appointment.Salon.Name}");
        sb.AppendLine($"Hizmet: {appointment.Service.Name}");
        sb.AppendLine($"Personel: {appointment.Staff.FirstName} {appointment.Staff.LastName}");
        sb.AppendLine($"Tarih: {appointment.StartTime:dd MMMM yyyy}");
        sb.AppendLine($"Saat: {appointment.StartTime:HH:mm} - {appointment.EndTime:HH:mm}");
        sb.AppendLine();
        sb.AppendLine("İyi günler dileriz,");
        sb.AppendLine("RendevumVar Ekibi");

        return sb.ToString();
    }
}
