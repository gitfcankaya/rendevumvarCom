namespace RendevumVar.Application.Services;

public interface IEmailService
{
    Task SendStaffInvitationAsync(string toEmail, string staffName, string invitationLink, CancellationToken cancellationToken = default);
    Task SendTimeOffApprovedAsync(string toEmail, string staffName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task SendTimeOffRejectedAsync(string toEmail, string staffName, DateTime startDate, DateTime endDate, string reason, CancellationToken cancellationToken = default);
}
