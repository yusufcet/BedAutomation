using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class LabParameter
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty; // mg/dL, g/dL, etc.
        
        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // HGB, GLU, etc.
        
        [StringLength(20)]
        public string DataType { get; set; } = "Numeric"; // Numeric, Text, Boolean
        
        public decimal? MinNormalValue { get; set; }
        public decimal? MaxNormalValue { get; set; }
        
        [StringLength(50)]
        public string NormalRange { get; set; } = string.Empty; // For complex ranges
        
        public int LabTestId { get; set; }
        public LabTest LabTest { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
        public ICollection<ReferenceRange> ReferenceRanges { get; set; } = new List<ReferenceRange>();
    }
} 