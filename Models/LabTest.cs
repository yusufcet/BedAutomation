using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class LabTest
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Blood, Urine, Biochemistry, etc.
        
        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // Lab code like CBC, BUN, etc.
        
        public decimal Price { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
        public ICollection<LabParameter> LabParameters { get; set; } = new List<LabParameter>();
    }
} 