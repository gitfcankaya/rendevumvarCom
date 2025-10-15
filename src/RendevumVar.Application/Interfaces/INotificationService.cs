namespace RendevumVar.Application.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Send appointment confirmation email to customer
    /// </summary>
    Task SendAppointmentConfirmationAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send appointment reminder 24 hours before the appointment
    /// </summary>
    Task SendAppointmentReminderAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send cancellation notification to both customer and salon
    /// </summary>
    Task SendCancellationNotificationAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send reschedule notification with old and new time
    /// </summary>
    Task SendRescheduleNotificationAsync(Guid appointmentId, DateTime oldStartTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send status update notification (e.g., confirmed, completed)
    /// </summary>
    Task SendStatusUpdateNotificationAsync(Guid appointmentId, string newStatus, CancellationToken cancellationToken = default);
}
