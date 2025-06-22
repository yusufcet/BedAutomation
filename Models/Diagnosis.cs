using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class Diagnosis
    {
        public int Id { get; set; }
        
        public int MedicalRecordId { get; set; }
        public MedicalRecord MedicalRecord { get; set; } = null!;
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string ICDCode { get; set; } = string.Empty; // ICD-10 code
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Type { get; set; } = "Primary"; // Primary, Secondary, Differential
        
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Resolved, Chronic
        
        [StringLength(20)]
        public string Severity { get; set; } = "Moderate"; // Mild, Moderate, Severe, Critical
        
        public DateTime DiagnosisDate { get; set; }
        
        public DateTime? ResolvedDate { get; set; }
        
        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 