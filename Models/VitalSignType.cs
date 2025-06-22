using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class VitalSignType
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty; // °C, mmHg, bpm, etc.
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public decimal? MinNormalValue { get; set; }
        public decimal? MaxNormalValue { get; set; }
        
        public decimal? CriticalMinValue { get; set; }
        public decimal? CriticalMaxValue { get; set; }
        
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Basic, Cardiac, Respiratory, etc.
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
    }
} 