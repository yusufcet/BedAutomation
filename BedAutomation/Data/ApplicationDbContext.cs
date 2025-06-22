using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BedAutomation.Models;

namespace BedAutomation.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        
        // Lab Results System
        public DbSet<LabTest> LabTests { get; set; }
        public DbSet<LabParameter> LabParameters { get; set; }
        public DbSet<LabResult> LabResults { get; set; }
        public DbSet<ReferenceRange> ReferenceRanges { get; set; }
        
        // Vital Signs System
        public DbSet<VitalSignType> VitalSignTypes { get; set; }
        public DbSet<VitalSign> VitalSigns { get; set; }
        
        // Prescription System
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }
        
        // Medical Records System
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<TreatmentPlan> TreatmentPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Patient configurations
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.IdentityNumber)
                .IsUnique();

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.Email)
                .IsUnique();

            // Doctor configurations
            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.IdentityNumber)
                .IsUnique();

            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.Email)
                .IsUnique();

            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.LicenseNumber)
                .IsUnique();

            // Room configurations
            modelBuilder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique();

            // Bed configurations
            modelBuilder.Entity<Bed>()
                .HasIndex(b => new { b.RoomId, b.BedNumber })
                .IsUnique();

            // Relationships
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Bed>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Beds)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Reservations)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Bed)
                .WithMany(b => b.Reservations)
                .HasForeignKey(r => r.BedId)
                .OnDelete(DeleteBehavior.Cascade);

            // Doctor relationships
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Doctor)
                .WithMany(d => d.Reservations)
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
