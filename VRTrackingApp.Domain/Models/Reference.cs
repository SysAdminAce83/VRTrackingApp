using System;

namespace VRTrackingApp.Domain.Models
{
    public class Reference
    {
        public int Id { get; set; }
        public int VulnerabilityId { get; set; }
        public string ReferenceType { get; set; } = default!;
        public string ReferenceValue { get; set; } = default!;
        public string? URL { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Vulnerability Vulnerability { get; set; } = default!;
    }
}