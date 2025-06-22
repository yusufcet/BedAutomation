using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class TreatmentPlan
    {
        public int Id { get; set; }
        
        public int MedicalRecordId { get; set; }
        public MedicalRecord MedicalRecord { get; set; } = null!;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Objectives { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Interventions { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent
        
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled, On Hold
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public DateTime? ReviewDate { get; set; }
        
        [StringLength(2000)]
        public string Progress { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Outcomes { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 