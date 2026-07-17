using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace VRTrackingApp.Web.Services.Notifications;

/// <summary>
/// SMTP email channel. Silently no-ops (returns false) when SMTP is not configured,
/// so the app stays fully functional in environments without email. Reads
/// <c>Email:Smtp:Host/Port/From/Username/Password/UseSsl</c> from configuration.
/// </summary>
public class EmailChannel : INotificationChannel
{
    private readonly EmailOptions _options;
    private readonly bool _enabled;

    public EmailChannel(IConfiguration configuration)
    {
        var section = configuration.GetSection("Email:Smtp");
        _options = new EmailOptions
        {
            Host = section["Host"] ?? "",
            Port = int.TryParse(section["Port"], out var p) ? p : 25,
            From = section["From"] ?? "",
            Username = section["Username"],
            Password = section["Password"],
            UseSsl = section["UseSsl"] != "false"
        };
        _enabled = !string.IsNullOrWhiteSpace(_options.Host) && !string.IsNullOrWhiteSpace(_options.From);
    }

    public bool Enabled => _enabled;

    public async Task<bool> SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(toEmail)) return false;
        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                Credentials = string.IsNullOrWhiteSpace(_options.Username)
                    ? null
                    : new NetworkCredential(_options.Username, _options.Password)
            };
            var mail = new MailMessage(_options.From, toEmail, subject, body) { IsBodyHtml = true };
            await client.SendMailAsync(mail);
            return true;
        }
        catch
        {
            // Never break the request flow because of email problems.
            return false;
        }
    }

    private sealed class EmailOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string From { get; set; } = "";
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool UseSsl { get; set; } = true;
    }
}
