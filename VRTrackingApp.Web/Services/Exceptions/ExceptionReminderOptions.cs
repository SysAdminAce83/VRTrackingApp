namespace VRTrackingApp.Web.Services.Exceptions;

/// <summary>Configuration for the exception lifecycle / reminder background job.</summary>
public class ExceptionReminderOptions
{
    public const string SectionName = "ExceptionLifecycle";
    /// <summary>Sweep interval in seconds (default 300 = 5 minutes).</summary>
    public int IntervalSeconds { get; set; } = 300;
}
