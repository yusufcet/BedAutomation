using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class Medication
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string GenericName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Manufacturer { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Form { get; set; } = string.Empty; // Tablet, Capsule, Syrup, etc.
        
        [StringLength(50)]
        public string Strength { get; set; } = string.Empty; // 500mg, 10ml, etc.
        
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Antibiotic, Analgesic, etc.
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string SideEffects { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Contraindications { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Interactions { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public int StockQuantity { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool RequiresPrescription { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
    }
} 