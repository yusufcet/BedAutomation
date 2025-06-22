using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class Room
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RoomNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string RoomType { get; set; } = string.Empty; // ICU, General, Private, Emergency
        
        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty; // Cardiology, Surgery, etc.
        
        public int Floor { get; set; }
        
        public int BedCapacity { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Bed> Beds { get; set; } = new List<Bed>();
    }
} 