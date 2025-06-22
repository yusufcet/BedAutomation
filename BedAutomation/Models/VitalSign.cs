using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class VitalSign
    {
        public int Id { get; set; }
        
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        
        public int VitalSignTypeId { get; set; }
        public VitalSignType VitalSignType { get; set; } = null!;
        
        public decimal Value { get; set; }
        
        [StringLength(100)]
        public string StringValue { get; set; } = string.Empty; // For complex values like "120/80"
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Normal"; // Normal, High, Low, Critical
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime MeasurementDate { get; set; }
        
        [StringLength(100)]
        public string MeasuredBy { get; set; } = string.Empty; // Nurse, Doctor name
        
        [StringLength(100)]
        public string Location { get; set; } = string.Empty; // Room, Ward
        
        public bool IsVerified { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 