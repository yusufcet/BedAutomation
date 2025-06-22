using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class ReferenceRange
    {
        public int Id { get; set; }
        
        public int LabParameterId { get; set; }
        public LabParameter LabParameter { get; set; } = null!;
        
        [StringLength(10)]
        public string Gender { get; set; } = "Both"; // Male, Female, Both
        
        public int MinAge { get; set; } = 0;
        public int MaxAge { get; set; } = 120;
        
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        
        [StringLength(100)]
        public string TextReference { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 