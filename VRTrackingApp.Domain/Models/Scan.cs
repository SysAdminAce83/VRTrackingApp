using System;
using System.Collections.Generic;

namespace VRTrackingApp.Domain.Models
{
    public class Scan
    {
        public int Id { get; set; }
        public string ScanName { get; set; } = default!;
        public DateTime ScanDate { get; set; }
        public string ScanType { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public long? FileSize { get; set; }
        public string? FileHash { get; set; }
        public string Status { get; set; } = "Processing";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}