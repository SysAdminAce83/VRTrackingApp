using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VRTrackingApp.Data.Models
{
    public class Standard
    {
        public int Id { get; set; }

        [Required]
        public int PolicyId { get; set; }
        public Policy Policy { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        public string Description { get; set; } = default!;

        [Required]
        public int Version { get; set; } = 1;

        [DataType(DataType.Date)]
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow.AddYears(1);

        public int? OwnerUserId { get; set; }
        public UserAccount? Owner { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = default!; // Draft, Under Review, Approved, Archived

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public UserAccount? CreatedBy { get; set; }
        public int? UpdatedByUserId { get; set; }
        public UserAccount? UpdatedBy { get; set; }
    }
}