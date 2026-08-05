using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VRTrackingApp.Data.Models
{
    public class Risk
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string RiskName { get; set; } = default!;

        [Required]
        public string Description { get; set; } = default!;

        /// <summary>
        /// Business Impact (e.g., Low, Medium, High, Critical)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string BusinessImpact { get; set; } = default!;

        /// <summary>
        /// Likelihood (e.g., Rare, Unlikely, Possible, Likely, Almost Certain)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Likelihood { get; set; } = default!;

        /// <summary>
        /// Risk Score (e.g., calculated from Impact and Likelihood, or manually set)
        /// </summary>
        [Required]
        public int RiskScore { get; set; }

        /// <summary>
        /// Owner of the risk (user responsible for managing the risk)
        /// </summary>
        public int? OwnerUserId { get; set; }
        public UserAccount? Owner { get; set; }

        /// <summary>
        /// Date when the risk is next due for review
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? ReviewDate { get; set; }

        /// <summary>
        /// Current status of the risk (e.g., Open, Mitigated, Closed, Accepted)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = default!;

        /// <summary>
        /// Additional comments or notes
        /// </summary>
        public string? Notes { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public UserAccount? CreatedBy { get; set; }
        public int? UpdatedByUserId { get; set; }
        public UserAccount? UpdatedBy { get; set; }
    }
}