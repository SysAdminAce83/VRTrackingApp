using System;

namespace VRTrackingApp.Data.Models;

/// <summary>P8 — link between an exception and an external ticketing system ticket (ServiceNow, Jira, etc.).</summary>
public class TicketingLink
{
    public int Id { get; set; }
    public int ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public string System { get; set; } = default!;
    public string TicketId { get; set; } = default!;
    public string? TicketUrl { get; set; }
    public string? Title { get; set; }
    public int? LinkedByUserId { get; set; }
    public UserAccount? LinkedBy { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}
