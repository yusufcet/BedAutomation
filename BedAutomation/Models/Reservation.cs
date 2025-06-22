using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        
        public int PatientId { get; set; }
        
        public int BedId { get; set; }
        
        [Required]
        public DateTime CheckInDate { get; set; }
        
        public DateTime? CheckOutDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Reserved"; // Reserved, Active, Completed, Cancelled
        
        [StringLength(100)]
        public string Priority { get; set; } = "Normal"; // Emergency, High, Normal, Low
        
        [StringLength(100)]
        public string AdmissionType { get; set; } = string.Empty; // Emergency, Scheduled, Transfer
        
        [StringLength(1000)]
        public string MedicalNotes { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string CancellationReason { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Optional doctor assignment
        public int? DoctorId { get; set; }

        // Navigation properties
        public Patient? Patient { get; set; }
        public Bed? Bed { get; set; }
        public Doctor? Doctor { get; set; }
    }
} 