namespace VRTrackingApp.Data.Models;

/// <summary>Section 9 — an existing security control selected for the exception.</summary>
public class ExceptionSecurityControl
{
    public int Id { get; set; }
    public int ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public string ControlName { get; set; } = default!;
}
