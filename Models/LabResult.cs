using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class LabResult
    {
        public int Id { get; set; }
        
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        
        public int LabTestId { get; set; }
        public LabTest LabTest { get; set; } = null!;
        
        public int LabParameterId { get; set; }
        public LabParameter LabParameter { get; set; } = null!;
        
        public decimal? NumericValue { get; set; }
        
        [StringLength(200)]
        public string TextValue { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Normal"; // Normal, High, Low, Critical
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime TestDate { get; set; }
        public DateTime ResultDate { get; set; }
        
        [StringLength(100)]
        public string TechnicianName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string DoctorName { get; set; } = string.Empty;
        
        public bool IsVerified { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 