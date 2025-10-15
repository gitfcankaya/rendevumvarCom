using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace RendevumVar.API.Hubs
{
    [Authorize]
    public class AppointmentHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Get user info from claims
            var userId = Context.User?.FindFirst("sub")?.Value
                      ?? Context.User?.FindFirst("userId")?.Value;

            var tenantId = Context.User?.FindFirst("tenantId")?.Value;
            var salonId = Context.User?.FindFirst("salonId")?.Value;

            // Add to user-specific group
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            // Add to tenant-specific group
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
            }

            // Add to salon-specific group (for staff/salon owner)
            if (!string.IsNullOrEmpty(salonId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"salon_{salonId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Groups are automatically removed on disconnect
            await base.OnDisconnectedAsync(exception);
        }

        // Client methods to receive notifications
        // These will be called from AppointmentService

        // When a new appointment is created
        public async Task NotifyAppointmentCreated(int appointmentId, string customerName, string serviceName, DateTime startTime)
        {
            // This is just a placeholder - actual notifications are sent from service layer
            await Task.CompletedTask;
        }

        // When an appointment is cancelled
        public async Task NotifyAppointmentCancelled(int appointmentId, string reason)
        {
            await Task.CompletedTask;
        }

        // When an appointment is rescheduled
        public async Task NotifyAppointmentRescheduled(int appointmentId, DateTime oldTime, DateTime newTime)
        {
            await Task.CompletedTask;
        }

        // When appointment status changes
        public async Task NotifyAppointmentStatusUpdated(int appointmentId, int oldStatus, int newStatus)
        {
            await Task.CompletedTask;
        }
    }
}
