using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Repositories;

namespace RendevumVar.API.BackgroundJobs;

public class AppointmentReminderJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentReminderJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public AppointmentReminderJob(
        IServiceProvider serviceProvider,
        ILogger<AppointmentReminderJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppointmentReminderJob is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing appointment reminders");
            }

            // Wait for the next interval
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("AppointmentReminderJob is stopping");
    }

    private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        // Get appointments that need reminders (24 hours before appointment time)
        var reminderTime = DateTime.UtcNow;
        var appointmentsNeedingReminders = await appointmentRepository.GetAppointmentsForRemindersAsync(
            reminderTime,
            cancellationToken);

        _logger.LogInformation("Found {Count} appointments needing reminders", appointmentsNeedingReminders.Count());

        foreach (var appointment in appointmentsNeedingReminders)
        {
            try
            {
                await notificationService.SendAppointmentReminderAsync(appointment.Id, cancellationToken);
                _logger.LogInformation("Reminder sent for appointment {AppointmentId}", appointment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder for appointment {AppointmentId}", appointment.Id);
            }
        }
    }
}
