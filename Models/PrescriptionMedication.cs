using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class PrescriptionMedication
    {
        public int Id { get; set; }
        
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;
        
        public int MedicationId { get; set; }
        public Medication Medication { get; set; } = null!;
        
        [StringLength(200)]
        public string Dosage { get; set; } = string.Empty; // "1 tablet", "5ml", etc.
        
        [StringLength(200)]
        public string Frequency { get; set; } = string.Empty; // "Twice daily", "Every 8 hours", etc.
        
        public int Duration { get; set; } // Duration in days
        
        public int Quantity { get; set; } // Total quantity prescribed
        
        [StringLength(500)]
        public string Instructions { get; set; } = string.Empty; // "Take with food", etc.
        
        [StringLength(500)]
        public string SideEffectsWarning { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 