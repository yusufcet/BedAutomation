using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BedAutomation.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Identity number is required")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Identity number must be 11 characters")]
        public string IdentityNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "License number is required")]
        [StringLength(20, ErrorMessage = "License number cannot exceed 20 characters")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Department cannot exceed 20 characters")]
        public string? Department { get; set; }

        [Range(1, 50, ErrorMessage = "Years of experience must be between 1 and 50")]
        public int YearsOfExperience { get; set; }

        [StringLength(500, ErrorMessage = "Biography cannot exceed 500 characters")]
        public string? Biography { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key to Identity User
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // Full name property for display
        public string FullName => $"{FirstName} {LastName}";
        public string DisplayName => $"Dr. {FullName}";
    }
} 