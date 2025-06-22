using System.ComponentModel.DataAnnotations;

namespace BedAutomation.Models
{
    public class Bed
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BedNumber { get; set; } = string.Empty;
        
        [Required]
        public int RoomId { get; set; }
        
        [StringLength(50)]
        public string BedType { get; set; } = string.Empty; // Standard, Electric, ICU
        
        public bool IsOccupied { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsMaintenanceRequired { get; set; } = false;
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Room Room { get; set; } = null!;
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
} 