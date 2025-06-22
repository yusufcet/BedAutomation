using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BedAutomation.Data;
using BedAutomation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BedAutomation.Controllers
{
    [Authorize]
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PrescriptionController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Prescription
        public async Task<IActionResult> Index(int? patientId, int? doctorId, string? status)
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName");
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName");
            ViewBag.StatusList = new SelectList(new[] { "Active", "Completed", "Cancelled" });

            var prescriptions = _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionMedications)
                    .ThenInclude(pm => pm.Medication)
                .AsQueryable();

            if (patientId.HasValue)
                prescriptions = prescriptions.Where(p => p.PatientId == patientId.Value);
            
            if (doctorId.HasValue)
                prescriptions = prescriptions.Where(p => p.DoctorId == doctorId.Value);
            
            if (!string.IsNullOrEmpty(status))
                prescriptions = prescriptions.Where(p => p.Status == status);

            // Statistics
            var totalPrescriptions = await prescriptions.CountAsync();
            var activePrescriptions = await prescriptions.CountAsync(p => p.Status == "Active");
            var dispensedPrescriptions = await prescriptions.CountAsync(p => p.IsDispensed);
            var expiredPrescriptions = await prescriptions.CountAsync(p => p.ExpiryDate < DateTime.UtcNow);

            ViewBag.TotalPrescriptions = totalPrescriptions;
            ViewBag.ActivePrescriptions = activePrescriptions;
            ViewBag.DispensedPrescriptions = dispensedPrescriptions;
            ViewBag.ExpiredPrescriptions = expiredPrescriptions;

            return View(await prescriptions.OrderByDescending(p => p.PrescriptionDate).ToListAsync());
        }

        // GET: Prescription/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionMedications)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prescription == null) return NotFound();

            return View(prescription);
        }

        // GET: Prescription/Create
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName");
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName");
            ViewBag.Medications = new SelectList(await _context.Medications.Where(m => m.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.StatusList = new SelectList(new[] { "Active", "Completed", "Cancelled" });
            
            var prescription = new Prescription
            {
                PrescriptionDate = DateTime.Now,
                PrescriptionNumber = GeneratePrescriptionNumber()
            };
            
            return View(prescription);
        }

        // POST: Prescription/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create(Prescription prescription)
        {
            // Clear default ModelState errors for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");
            ModelState.Remove("PrescriptionMedications");
            
            // Manual validation for required fields
            if (prescription.PatientId <= 0)
                ModelState.AddModelError("PatientId", "Please select a patient.");
            if (prescription.DoctorId <= 0)
                ModelState.AddModelError("DoctorId", "Please select a doctor.");
            if (string.IsNullOrEmpty(prescription.Status))
                ModelState.AddModelError("Status", "Please select a status.");
            
            if (ModelState.IsValid)
            {
                // Ensure UTC dates
                if (prescription.PrescriptionDate.Kind == DateTimeKind.Unspecified)
                    prescription.PrescriptionDate = DateTime.SpecifyKind(prescription.PrescriptionDate, DateTimeKind.Utc);
                if (prescription.ExpiryDate.HasValue && prescription.ExpiryDate.Value.Kind == DateTimeKind.Unspecified)
                    prescription.ExpiryDate = DateTime.SpecifyKind(prescription.ExpiryDate.Value, DateTimeKind.Utc);
                
                prescription.CreatedAt = DateTime.UtcNow;
                _context.Add(prescription);
                await _context.SaveChangesAsync();
                
                // Prescription oluşturulduktan sonra medication ekleme için Edit sayfasına yönlendir
                TempData["SuccessMessage"] = "Reçete başarıyla oluşturuldu. Şimdi ilaç ekleyebilirsiniz.";
                return RedirectToAction(nameof(Edit), new { id = prescription.Id });
            }
            
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", prescription.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName", prescription.DoctorId);
            ViewBag.StatusList = new SelectList(new[] { "Active", "Completed", "Cancelled" }, prescription.Status);
            return View(prescription);
        }

        // GET: Prescription/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionMedications)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (prescription == null) return NotFound();

            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", prescription.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName", prescription.DoctorId);
            ViewBag.Medications = new SelectList(await _context.Medications.Where(m => m.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.StatusList = new SelectList(new[] { "Active", "Completed", "Cancelled" }, prescription.Status);
            return View(prescription);
        }

        // POST: Prescription/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, Prescription prescription)
        {
            if (id != prescription.Id) return NotFound();

            // Clear default ModelState errors for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");
            ModelState.Remove("PrescriptionMedications");

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure UTC dates
                    if (prescription.PrescriptionDate.Kind == DateTimeKind.Unspecified)
                        prescription.PrescriptionDate = DateTime.SpecifyKind(prescription.PrescriptionDate, DateTimeKind.Utc);
                    if (prescription.ExpiryDate.HasValue && prescription.ExpiryDate.Value.Kind == DateTimeKind.Unspecified)
                        prescription.ExpiryDate = DateTime.SpecifyKind(prescription.ExpiryDate.Value, DateTimeKind.Utc);
                    if (prescription.DispensedDate.HasValue && prescription.DispensedDate.Value.Kind == DateTimeKind.Unspecified)
                        prescription.DispensedDate = DateTime.SpecifyKind(prescription.DispensedDate.Value, DateTimeKind.Utc);
                    
                    prescription.UpdatedAt = DateTime.UtcNow;
                    _context.Update(prescription);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionExists(prescription.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", prescription.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName", prescription.DoctorId);
            ViewBag.StatusList = new SelectList(new[] { "Active", "Completed", "Cancelled" }, prescription.Status);
            return View(prescription);
        }

        // GET: Prescription/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionMedications)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prescription == null) return NotFound();

            return View(prescription);
        }

        // POST: Prescription/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionMedications)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (prescription != null)
            {
                _context.PrescriptionMedications.RemoveRange(prescription.PrescriptionMedications);
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Prescription/ManageMedications/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> ManageMedications(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionMedications)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return NotFound();

            ViewBag.Medications = new SelectList(await _context.Medications.Where(m => m.IsActive).ToListAsync(), "Id", "Name");
            return View(prescription);
        }

        // POST: Add Medication to Prescription
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AddMedication(int prescriptionId, int medicationId, string dosage, string frequency, int duration, int quantity, string instructions, string startDate, string endDate)
        {
            var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
            var medication = await _context.Medications.FindAsync(medicationId);
            
            if (prescription == null || medication == null) return NotFound();

            // Parse and convert dates to UTC
            var parsedStartDate = DateTime.Parse(startDate);
            var parsedEndDate = DateTime.Parse(endDate);
            
            if (parsedStartDate.Kind == DateTimeKind.Unspecified)
                parsedStartDate = DateTime.SpecifyKind(parsedStartDate, DateTimeKind.Utc);
            
            if (parsedEndDate.Kind == DateTimeKind.Unspecified)
                parsedEndDate = DateTime.SpecifyKind(parsedEndDate, DateTimeKind.Utc);

            var prescriptionMedication = new PrescriptionMedication
            {
                PrescriptionId = prescriptionId,
                MedicationId = medicationId,
                Dosage = dosage,
                Frequency = frequency,
                Duration = duration,
                Quantity = quantity,
                Instructions = instructions,
                StartDate = parsedStartDate,
                EndDate = parsedEndDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.PrescriptionMedications.Add(prescriptionMedication);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageMedications), new { id = prescriptionId });
        }

        // POST: Remove Medication from Prescription
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> RemoveMedication(int prescriptionMedicationId)
        {
            var prescriptionMedication = await _context.PrescriptionMedications.FindAsync(prescriptionMedicationId);
            if (prescriptionMedication == null) return NotFound();

            var prescriptionId = prescriptionMedication.PrescriptionId;
            _context.PrescriptionMedications.Remove(prescriptionMedication);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageMedications), new { id = prescriptionId });
        }

        private bool PrescriptionExists(int id)
        {
            return _context.Prescriptions.Any(e => e.Id == id);
        }

        private string GeneratePrescriptionNumber()
        {
            return "RX" + DateTime.Now.ToString("yyyyMMdd") + new Random().Next(1000, 9999).ToString();
        }
    }
} 