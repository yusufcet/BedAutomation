using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BedAutomation.Models
{
    public class Patient
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(11)]
        public string IdentityNumber { get; set; } = string.Empty; // TC Kimlik No
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty; // Male, Female, Other
        
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // Display properties
        public string FullName => $"{FirstName} {LastName}";
    }
} 