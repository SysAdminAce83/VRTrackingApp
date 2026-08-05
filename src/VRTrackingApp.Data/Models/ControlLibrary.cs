using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VRTrackingApp.Data.Models
{
    public class ControlLibrary
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ControlId { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Domain { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string ControlName { get; set; } = default!;

        [Required]
        public string ControlDescription { get; set; } = default!;

        [StringLength(200)]
        public string? Objective { get; set; }

        [StringLength(100)]
        public string? ControlOwner { get; set; }

        [StringLength(50)]
        public string? Frequency { get; set; }

        [StringLength(500)]
        public string? Evidence { get; set; }

        [StringLength(1000)]
        public string? TestSteps { get; set; }

        [StringLength(200)]
        public string? RiskAddressed { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public UserAccount? CreatedBy { get; set; }
        public int? UpdatedByUserId { get; set; }
        public UserAccount? UpdatedBy { get; set; }
    }
}
