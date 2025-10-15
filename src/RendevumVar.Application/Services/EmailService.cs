using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RendevumVar.Core.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace RendevumVar.Application.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task SendStaffInvitationAsync(
        string toEmail,
        string staffName,
        string invitationLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "RendevumVar - Personel Davetiyesi";
            var body = GetStaffInvitationEmailBody(staffName, invitationLink);

            await SendEmailAsync(toEmail, subject, body, cancellationToken);
            
            _logger.LogInformation("Staff invitation email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send staff invitation email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendTimeOffApprovedAsync(
        string toEmail,
        string staffName,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "RendevumVar - İzin Talebiniz Onaylandı";
            var body = GetTimeOffApprovedEmailBody(staffName, startDate, endDate);

            await SendEmailAsync(toEmail, subject, body, cancellationToken);
            
            _logger.LogInformation("Time off approval email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send time off approval email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendTimeOffRejectedAsync(
        string toEmail,
        string staffName,
        DateTime startDate,
        DateTime endDate,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "RendevumVar - İzin Talebiniz Reddedildi";
            var body = GetTimeOffRejectedEmailBody(staffName, startDate, endDate, reason);

            await SendEmailAsync(toEmail, subject, body, cancellationToken);
            
            _logger.LogInformation("Time off rejection email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send time off rejection email to {Email}", toEmail);
            throw;
        }
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        // Check if email settings are configured
        if (string.IsNullOrEmpty(_emailSettings.SmtpHost) || 
            string.IsNullOrEmpty(_emailSettings.Username))
        {
            _logger.LogWarning("Email settings not configured. Email would have been sent to {Email} with subject: {Subject}", 
                toEmail, subject);
            return;
        }

        using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
        {
            EnableSsl = _emailSettings.EnableSsl,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage, cancellationToken);
    }

    #region Email Templates

    private string GetStaffInvitationEmailBody(string staffName, string invitationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Personel Davetiyesi</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8f9fa; border-radius: 10px; padding: 30px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-bottom: 20px;'>🎉 RendevumVar'a Hoş Geldiniz!</h1>
        
        <p style='font-size: 16px; margin-bottom: 15px;'>Merhaba <strong>{staffName}</strong>,</p>
        
        <p style='font-size: 16px; margin-bottom: 15px;'>
            RendevumVar sistemine personel olarak davet edildiniz. Hesabınızı aktif etmek ve sisteme giriş yapmak için 
            aşağıdaki butona tıklayınız.
        </p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{invitationLink}' 
               style='background-color: #3498db; color: white; padding: 15px 40px; text-decoration: none; 
                      border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                Hesabı Aktif Et
            </a>
        </div>
        
        <p style='font-size: 14px; color: #7f8c8d; margin-top: 30px;'>
            Eğer butona tıklayamıyorsanız, aşağıdaki linki tarayıcınıza kopyalayın:
        </p>
        <p style='font-size: 12px; color: #95a5a6; word-break: break-all; background-color: #ecf0f1; 
                  padding: 10px; border-radius: 5px;'>
            {invitationLink}
        </p>
        
        <p style='font-size: 14px; color: #e74c3c; margin-top: 20px;'>
            ⚠️ Bu davetiye linki 7 gün süreyle geçerlidir.
        </p>
    </div>
    
    <div style='text-align: center; color: #95a5a6; font-size: 12px; margin-top: 20px;'>
        <p>© 2025 RendevumVar - Randevu Yönetim Sistemi</p>
        <p>Bu bir otomatik mesajdır, lütfen yanıtlamayınız.</p>
    </div>
</body>
</html>";
    }

    private string GetTimeOffApprovedEmailBody(string staffName, DateTime startDate, DateTime endDate)
    {
        var days = (endDate - startDate).Days + 1;
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>İzin Onayı</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #d4edda; border-radius: 10px; padding: 30px; margin-bottom: 20px; border: 2px solid #28a745;'>
        <h1 style='color: #155724; margin-bottom: 20px;'>✅ İzin Talebiniz Onaylandı</h1>
        
        <p style='font-size: 16px; margin-bottom: 15px;'>Merhaba <strong>{staffName}</strong>,</p>
        
        <p style='font-size: 16px; margin-bottom: 15px;'>
            İzin talebiniz onaylandı. İzin detayları aşağıdaki gibidir:
        </p>
        
        <div style='background-color: white; border-radius: 5px; padding: 20px; margin: 20px 0;'>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Başlangıç Tarihi:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6; text-align: right;'>{startDate:dd MMMM yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Bitiş Tarihi:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6; text-align: right;'>{endDate:dd MMMM yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 10px;'><strong>Toplam Gün:</strong></td>
                    <td style='padding: 10px; text-align: right; color: #28a745; font-weight: bold;'>{days} gün</td>
                </tr>
            </table>
        </div>
        
        <p style='font-size: 16px; margin-top: 20px;'>
            İyi tatiller! 🌴
        </p>
    </div>
    
    <div style='text-align: center; color: #95a5a6; font-size: 12px; margin-top: 20px;'>
        <p>© 2025 RendevumVar - Randevu Yönetim Sistemi</p>
        <p>Bu bir otomatik mesajdır, lütfen yanıtlamayınız.</p>
    </div>
</body>
</html>";
    }

    private string GetTimeOffRejectedEmailBody(string staffName, DateTime startDate, DateTime endDate, string reason)
    {
        var days = (endDate - startDate).Days + 1;
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>İzin Reddi</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8d7da; border-radius: 10px; padding: 30px; margin-bottom: 20px; border: 2px solid #dc3545;'>
        <h1 style='color: #721c24; margin-bottom: 20px;'>❌ İzin Talebiniz Reddedildi</h1>
        
        <p style='font-size: 16px; margin-bottom: 15px;'>Merhaba <strong>{staffName}</strong>,</p>
        
        <p style='font-size: 16px; margin-bottom: 15px;'>
            Maalesef izin talebiniz reddedildi. İzin detayları aşağıdaki gibidir:
        </p>
        
        <div style='background-color: white; border-radius: 5px; padding: 20px; margin: 20px 0;'>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Başlangıç Tarihi:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6; text-align: right;'>{startDate:dd MMMM yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Bitiş Tarihi:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6; text-align: right;'>{endDate:dd MMMM yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Toplam Gün:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6; text-align: right;'>{days} gün</td>
                </tr>
                <tr>
                    <td style='padding: 10px;' colspan='2'>
                        <strong>Ret Nedeni:</strong><br>
                        <div style='margin-top: 10px; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #dc3545; border-radius: 3px;'>
                            {reason}
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        
        <p style='font-size: 16px; margin-top: 20px;'>
            Detaylar için lütfen yöneticinizle görüşün.
        </p>
    </div>
    
    <div style='text-align: center; color: #95a5a6; font-size: 12px; margin-top: 20px;'>
        <p>© 2025 RendevumVar - Randevu Yönetim Sistemi</p>
        <p>Bu bir otomatik mesajdır, lütfen yanıtlamayınız.</p>
    </div>
</body>
</html>";
    }

    #endregion
}
