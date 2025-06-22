using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        
        public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        
        [StringLength(20)]
        public string PrescriptionNumber { get; set; } = string.Empty;
        
        public DateTime PrescriptionDate { get; set; }
        
        [StringLength(1000)]
        public string Diagnosis { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Instructions { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsDispensed { get; set; } = false;
        
        public DateTime? DispensedDate { get; set; }
        
        [StringLength(100)]
        public string PharmacistName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
    }
} 