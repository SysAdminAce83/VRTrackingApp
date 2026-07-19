namespace VRTrackingApp.Web.Html;

public static class UiHelpers
{
    public static string Pill(string status)
    {
        var s = (status ?? "").ToLowerInvariant();
        var cls = s switch
        {
            "open" => "pill-open",
            "fixed" => "pill-fixed",
            "exception" => "pill-exception",
            "completed" => "pill-completed",
            "processing" => "pill-processing",
            "failed" => "pill-failed",
            "awaiting parsing" => "pill-processing",
            _ => "pill-open"
        };
        var label = string.IsNullOrWhiteSpace(status) ? "—" : status;
        return $"<span class=\"pill {cls}\">{label}</span>";
    }

    public static string Sev(string severity)
    {
        var s = (severity ?? "").ToLowerInvariant();
        var cls = s switch
        {
            "critical" => "sev-critical",
            "high" => "sev-high",
            "medium" => "sev-medium",
            "low" => "sev-low",
            "none" or "info" => "sev-none",
            _ => "sev-none"
        };
        var label = string.IsNullOrWhiteSpace(severity) ? "—" : severity;
        return $"<span class=\"badge {cls}\">{label}</span>";
    }

    /// <summary>Turns PascalCase enum names into friendly labels (e.g. VeryLow -> "Very Low").</summary>
    public static string Humanize(this Enum value)
    {
        var name = value.ToString();
        var result = System.Text.RegularExpressions.Regex.Replace(name, "(?<!^)(?=[A-Z])", " ");
        return result;
    }
    public static Microsoft.AspNetCore.Html.IHtmlContent StatusBadge(VRTrackingApp.Data.Models.ExceptionStatus s)
    {
        var cls = s switch
        {
            VRTrackingApp.Data.Models.ExceptionStatus.ActiveException => "st-active",
            VRTrackingApp.Data.Models.ExceptionStatus.Renewed => "st-active",
            VRTrackingApp.Data.Models.ExceptionStatus.PendingTechnicalApproval or VRTrackingApp.Data.Models.ExceptionStatus.PendingManagerApproval or VRTrackingApp.Data.Models.ExceptionStatus.PendingSecurityApproval => "st-pending",
            VRTrackingApp.Data.Models.ExceptionStatus.ExceptionRequested or VRTrackingApp.Data.Models.ExceptionStatus.UnderReview or VRTrackingApp.Data.Models.ExceptionStatus.Detected => "st-pending",
            VRTrackingApp.Data.Models.ExceptionStatus.ReviewDue => "st-warn",
            VRTrackingApp.Data.Models.ExceptionStatus.Expired => "st-expired",
            VRTrackingApp.Data.Models.ExceptionStatus.Rejected => "st-rejected",
            VRTrackingApp.Data.Models.ExceptionStatus.Closed => "st-closed",
            VRTrackingApp.Data.Models.ExceptionStatus.NeedMoreInfo => "st-warn",
            _ => "st-closed"
        };
        return new Microsoft.AspNetCore.Html.HtmlString($"<span class=\"status-badge {cls}\">{VRTrackingApp.Data.Models.ExceptionStatusLabels.For(s)}</span>");
    }

    public static Microsoft.AspNetCore.Html.IHtmlContent RiskBadge(VRTrackingApp.Data.Models.RiskLevel? r)
    {
        if (r == null) return new Microsoft.AspNetCore.Html.HtmlString("<span class=\"text-muted\">—</span>");
        var cls = r switch
        {
            VRTrackingApp.Data.Models.RiskLevel.Low => "risk-low",
            VRTrackingApp.Data.Models.RiskLevel.Medium => "risk-medium",
            VRTrackingApp.Data.Models.RiskLevel.High => "risk-high",
            _ => "risk-critical"
        };
        return new Microsoft.AspNetCore.Html.HtmlString($"<span class=\"risk-badge {cls}\">{r}</span>");
    }
}
