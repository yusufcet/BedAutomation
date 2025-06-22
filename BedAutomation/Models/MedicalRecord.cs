using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        
        public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        
        [StringLength(50)]
        public string RecordType { get; set; } = string.Empty; // Consultation, Emergency, Surgery, etc.
        
        [Required]
        [StringLength(2000)]
        public string ChiefComplaint { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string HistoryOfPresentIllness { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string PhysicalExamination { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Assessment { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Plan { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime RecordDate { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Closed, Pending
        
        [StringLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical
        
        public DateTime? AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        
        [StringLength(2000)]
        public string DischargeSummary { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;
        
        public bool IsConfidential { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
        public ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
    }
} 