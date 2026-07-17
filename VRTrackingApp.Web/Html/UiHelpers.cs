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
}
