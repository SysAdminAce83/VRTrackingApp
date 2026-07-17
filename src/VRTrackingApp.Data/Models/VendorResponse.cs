using System;

namespace VRTrackingApp.Data.Models;

/// <summary>Vendor response / patch ETA captured against an exception.</summary>
public class VendorResponse
{
    public int Id { get; set; }
    public int ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public string? Vendor { get; set; }
    public string? ResponseText { get; set; }
    public DateTime? PatchEtaDate { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
