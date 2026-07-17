using System;
using System.Net;
using System.Text;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Notifications;

/// <summary>
/// Builds professional, table-based HTML email bodies for each
/// <see cref="NotificationType"/>. Output is intentionally inline-styled and
/// table-layout for maximum compatibility across mail clients (Outlook, Gmail,
/// Apple Mail). All dynamic text is HTML-escaped to prevent injection.
/// </summary>
public class EmailTemplateService
{
    /// <summary>
    /// Render a branded notification email.
    /// </summary>
    /// <param name="type">Drives the accent colour, icon and heading.</param>
    /// <param name="title">Email subject / headline (also used as the H1).</param>
    /// <param name="message">Body text (may contain a single line of plain prose).</param>
    /// <param name="reference">Optional short reference line, e.g. "Exception #12".</param>
    /// <param name="actionUrl">Optional deep link rendered as a button.</param>
    /// <param name="recipientName">Optional friendly name for the greeting.</param>
    public string Render(NotificationType type, string title, string message,
        string? reference, string? actionUrl, string? recipientName)
    {
        var (accent, heading, icon) = ThemeFor(type);
        var greeter = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName!;
        var button = string.IsNullOrWhiteSpace(actionUrl)
            ? ""
            : $"<a href=\"{EscapeAttr(actionUrl)}\" style=\"display:inline-block;background:{accent};color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:11px 22px;border-radius:8px;\">Review in console</a>";
        var refLine = string.IsNullOrWhiteSpace(reference) ? "" : $"{EscapeHtml(reference)} · ";

        var sb = new StringBuilder();
        sb.Append("<!doctype html>");
        sb.Append("<html lang=\"en\"><head><meta charset=\"utf-8\" />");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\" />");
        sb.Append("<title>").Append(EscapeHtml(title)).Append("</title></head>");
        sb.Append("<body style=\"margin:0;padding:0;background:#f1f5f9;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;\">");
        sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#f1f5f9;padding:24px 0;\"><tr><td align=\"center\">");
        sb.Append("<table role=\"presentation\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:600px;max-width:100%;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 1px 3px rgba(16,24,40,.12);\">");

        // Brand header
        sb.Append("<tr><td style=\"background:").Append(accent).Append(";padding:18px 28px;\">");
        sb.Append("<span style=\"color:#ffffff;font-size:16px;font-weight:700;letter-spacing:.4px;\">VR Remediation Console</span>");
        sb.Append("</td></tr>");

        // Body
        sb.Append("<tr><td style=\"padding:26px 28px 8px;\">");
        sb.Append("<div style=\"display:inline-block;background:").Append(accent).Append("1a;color:").Append(accent)
            .Append(";font-size:12px;font-weight:700;padding:6px 12px;border-radius:999px;letter-spacing:.3px;\">")
            .Append(EscapeHtml(icon)).Append(' ').Append(EscapeHtml(heading)).Append("</div>");
        sb.Append("<h1 style=\"font-size:20px;line-height:1.3;margin:14px 0 6px;color:#1f2733;font-weight:700;\">").Append(EscapeHtml(title)).Append("</h1>");
        sb.Append("<p style=\"font-size:14px;line-height:1.5;color:#475569;margin:0 0 14px;\">Hi ").Append(EscapeHtml(greeter)).Append(",</p>");
        sb.Append("<p style=\"font-size:14px;line-height:1.6;color:#334155;margin:0 0 20px;white-space:pre-wrap;\">").Append(EscapeHtml(message)).Append("</p>");
        if (!string.IsNullOrEmpty(button))
            sb.Append("<p style=\"margin:0 0 8px;\">").Append(button).Append("</p>");
        sb.Append("</td></tr>");

        // Footer
        sb.Append("<tr><td style=\"padding:18px 28px;border-top:1px solid #e3e7ee;\">");
        sb.Append("<p style=\"font-size:12px;line-height:1.5;color:#94a3b8;margin:0;\">")
            .Append(refLine)
            .Append("This is an automated message from the VR Remediation Console. Please do not reply to this email.")
            .Append("</p>");
        sb.Append("</td></tr>");

        sb.Append("</table></td></tr></table></body></html>");
        return sb.ToString();
    }

    private static (string Accent, string Heading, string Icon) ThemeFor(NotificationType t) => t switch
    {
        NotificationType.NewExceptionRequest or NotificationType.ApprovalRequired => ("#2563eb", "Approval Required", "⏳"),
        NotificationType.ExceptionApproved => ("#16a34a", "Exception Approved", "✓"),
        NotificationType.RequestRejected => ("#dc2626", "Exception Rejected", "✕"),
        NotificationType.ExceptionExpiring or NotificationType.ExceptionExpired => ("#ea580c", "Expiry Notice", "⏰"),
        NotificationType.ReviewDue => ("#ca8a04", "Periodic Review Due", "🔍"),
        NotificationType.EvidenceMissing => ("#9333ea", "Evidence Missing", "📎"),
        NotificationType.MitigationOverdue => ("#c2410c", "Mitigation Overdue", "🛠"),
        NotificationType.NeedMoreInfo => ("#0ea5e9", "More Information Requested", "✎"),
        _ => ("#2563eb", "Notification", "•")
    };

    private static string EscapeHtml(string? value) =>
        WebUtility.HtmlEncode(value ?? "");

    private static string EscapeAttr(string? value) =>
        WebUtility.HtmlEncode(value ?? "").Replace("'", "&#39;");
}
