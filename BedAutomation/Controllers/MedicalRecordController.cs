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
    public class MedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MedicalRecordController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MedicalRecord
        public async Task<IActionResult> Index(int? patientId, int? doctorId, string? recordType)
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName");
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName");
            ViewBag.RecordTypes = new SelectList(new[] { "Admission", "Discharge", "Emergency", "Consultation", "Follow-up" });

            var medicalRecords = _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Diagnoses)
                .Include(m => m.TreatmentPlans)
                .AsQueryable();

            if (patientId.HasValue)
                medicalRecords = medicalRecords.Where(m => m.PatientId == patientId.Value);
            
            if (doctorId.HasValue)
                medicalRecords = medicalRecords.Where(m => m.DoctorId == doctorId.Value);
            
            if (!string.IsNullOrEmpty(recordType))
                medicalRecords = medicalRecords.Where(m => m.RecordType == recordType);

            // Statistics
            var totalRecords = await medicalRecords.CountAsync();
            var activePatients = await medicalRecords.Select(m => m.PatientId).Distinct().CountAsync();
            var criticalRecords = await medicalRecords.CountAsync(m => m.Priority == "Critical");
            var recentRecords = await medicalRecords.CountAsync(m => m.RecordDate >= DateTime.UtcNow.AddDays(-30));

            ViewBag.TotalRecords = totalRecords;
            ViewBag.ActivePatients = activePatients;
            ViewBag.CriticalRecords = criticalRecords;
            ViewBag.RecentRecords = recentRecords;

            return View(await medicalRecords.OrderByDescending(m => m.RecordDate).ToListAsync());
        }

        // GET: MedicalRecord/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Diagnoses)
                .Include(m => m.TreatmentPlans)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null) return NotFound();

            return View(medicalRecord);
        }

        // GET: MedicalRecord/Create
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName");
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName");
            ViewBag.RecordTypes = new SelectList(new[] { "Admission", "Discharge", "Emergency", "Consultation", "Follow-up" });
            ViewBag.StatusList = new SelectList(new[] { "Active", "Closed", "Pending" });
            ViewBag.PriorityList = new SelectList(new[] { "Low", "Normal", "High", "Critical" });
            
            var medicalRecord = new MedicalRecord
            {
                RecordDate = DateTime.Now
            };
            
            return View(medicalRecord);
        }

        // POST: MedicalRecord/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create(MedicalRecord medicalRecord)
        {
            // Clear default ModelState errors for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");
            ModelState.Remove("Diagnoses");
            ModelState.Remove("TreatmentPlans");
            
            // Manual validation for required fields
            if (medicalRecord.PatientId <= 0)
                ModelState.AddModelError("PatientId", "Please select a patient.");
            if (medicalRecord.DoctorId <= 0)
                ModelState.AddModelError("DoctorId", "Please select a doctor.");
            if (string.IsNullOrEmpty(medicalRecord.RecordType))
                ModelState.AddModelError("RecordType", "Please select a record type.");
            if (string.IsNullOrEmpty(medicalRecord.Status))
                ModelState.AddModelError("Status", "Please select a status.");
            
            if (ModelState.IsValid)
            {
                // Ensure UTC dates
                if (medicalRecord.RecordDate.Kind == DateTimeKind.Unspecified)
                    medicalRecord.RecordDate = DateTime.SpecifyKind(medicalRecord.RecordDate, DateTimeKind.Utc);
                
                if (medicalRecord.AdmissionDate.HasValue && medicalRecord.AdmissionDate.Value.Kind == DateTimeKind.Unspecified)
                    medicalRecord.AdmissionDate = DateTime.SpecifyKind(medicalRecord.AdmissionDate.Value, DateTimeKind.Utc);
                
                if (medicalRecord.DischargeDate.HasValue && medicalRecord.DischargeDate.Value.Kind == DateTimeKind.Unspecified)
                    medicalRecord.DischargeDate = DateTime.SpecifyKind(medicalRecord.DischargeDate.Value, DateTimeKind.Utc);
                
                medicalRecord.CreatedAt = DateTime.UtcNow;
                _context.Add(medicalRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", medicalRecord.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName", medicalRecord.DoctorId);
            ViewBag.RecordTypes = new SelectList(new[] { "Admission", "Discharge", "Emergency", "Consultation", "Follow-up" }, medicalRecord.RecordType);
            ViewBag.StatusList = new SelectList(new[] { "Active", "Closed", "Pending" }, medicalRecord.Status);
            ViewBag.PriorityList = new SelectList(new[] { "Low", "Normal", "High", "Critical" }, medicalRecord.Priority);
            return View(medicalRecord);
        }

        // GET: MedicalRecord/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Diagnoses)
                .Include(m => m.TreatmentPlans)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (medicalRecord == null) return NotFound();

            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", medicalRecord.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName", medicalRecord.DoctorId);
            ViewBag.RecordTypes = new SelectList(new[] { "Admission", "Discharge", "Emergency", "Consultation", "Follow-up" }, medicalRecord.RecordType);
            ViewBag.StatusList = new SelectList(new[] { "Active", "Closed", "Pending" }, medicalRecord.Status);
            ViewBag.PriorityList = new SelectList(new[] { "Low", "Normal", "High", "Critical" }, medicalRecord.Priority);
            return View(medicalRecord);
        }

        // POST: MedicalRecord/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, MedicalRecord medicalRecord)
        {
            if (id != medicalRecord.Id) return NotFound();

            // Clear default ModelState errors for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");
            ModelState.Remove("Diagnoses");
            ModelState.Remove("TreatmentPlans");

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure UTC dates
                    if (medicalRecord.RecordDate.Kind == DateTimeKind.Unspecified)
                        medicalRecord.RecordDate = DateTime.SpecifyKind(medicalRecord.RecordDate, DateTimeKind.Utc);
                    
                    if (medicalRecord.AdmissionDate.HasValue && medicalRecord.AdmissionDate.Value.Kind == DateTimeKind.Unspecified)
                        medicalRecord.AdmissionDate = DateTime.SpecifyKind(medicalRecord.AdmissionDate.Value, DateTimeKind.Utc);
                    
                    if (medicalRecord.DischargeDate.HasValue && medicalRecord.DischargeDate.Value.Kind == DateTimeKind.Unspecified)
                        medicalRecord.DischargeDate = DateTime.SpecifyKind(medicalRecord.DischargeDate.Value, DateTimeKind.Utc);
                    
                    medicalRecord.UpdatedAt = DateTime.UtcNow;
                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalRecordExists(medicalRecord.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", medicalRecord.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "FullName", medicalRecord.DoctorId);
            ViewBag.RecordTypes = new SelectList(new[] { "Admission", "Discharge", "Emergency", "Consultation", "Follow-up" }, medicalRecord.RecordType);
            ViewBag.StatusList = new SelectList(new[] { "Active", "Closed", "Pending" }, medicalRecord.Status);
            ViewBag.PriorityList = new SelectList(new[] { "Low", "Normal", "High", "Critical" }, medicalRecord.Priority);
            return View(medicalRecord);
        }

        // GET: MedicalRecord/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Diagnoses)
                .Include(m => m.TreatmentPlans)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null) return NotFound();

            return View(medicalRecord);
        }

        // POST: MedicalRecord/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Diagnoses)
                .Include(m => m.TreatmentPlans)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (medicalRecord != null)
            {
                // Remove related diagnoses and treatment plans first
                _context.Diagnoses.RemoveRange(medicalRecord.Diagnoses);
                _context.TreatmentPlans.RemoveRange(medicalRecord.TreatmentPlans);
                _context.MedicalRecords.Remove(medicalRecord);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Manage Diagnoses for a Medical Record
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> ManageDiagnoses(int id)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Diagnoses)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null) return NotFound();

            return View(medicalRecord);
        }

        // Add Diagnosis
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AddDiagnosis(int medicalRecordId, string name, string icdCode, string description, string type, string status, string severity, string diagnosisDate, string notes)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (medicalRecord == null) return NotFound();

            var diagnosis = new Diagnosis
            {
                MedicalRecordId = medicalRecordId,
                Name = name,
                ICDCode = icdCode,
                Description = description,
                Type = type,
                Status = status,
                Severity = severity,
                DiagnosisDate = DateTime.Parse(diagnosisDate),
                Notes = notes
            };

            _context.Diagnoses.Add(diagnosis);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageDiagnoses), new { id = medicalRecordId });
        }

        // Add Treatment Plan
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AddTreatmentPlan(int medicalRecordId, string title, string description, string objectives, string interventions, string priority, string status, string startDate, string endDate, string reviewDate)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (medicalRecord == null) return NotFound();

            var treatmentPlan = new TreatmentPlan
            {
                MedicalRecordId = medicalRecordId,
                Title = title,
                Description = description,
                Objectives = objectives,
                Interventions = interventions,
                Priority = priority,
                Status = status,
                StartDate = DateTime.Parse(startDate),
                EndDate = string.IsNullOrEmpty(endDate) ? null : DateTime.Parse(endDate),
                ReviewDate = string.IsNullOrEmpty(reviewDate) ? null : DateTime.Parse(reviewDate)
            };

            _context.TreatmentPlans.Add(treatmentPlan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageDiagnoses), new { id = medicalRecordId });
        }

        // Remove Diagnosis
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> RemoveDiagnosis(int diagnosisId)
        {
            var diagnosis = await _context.Diagnoses.FindAsync(diagnosisId);
            if (diagnosis != null)
            {
                var medicalRecordId = diagnosis.MedicalRecordId;
                _context.Diagnoses.Remove(diagnosis);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageDiagnoses), new { id = medicalRecordId });
            }
            return RedirectToAction(nameof(Index));
        }

        // Remove Treatment Plan
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> RemoveTreatmentPlan(int treatmentPlanId)
        {
            var treatmentPlan = await _context.TreatmentPlans.FindAsync(treatmentPlanId);
            if (treatmentPlan != null)
            {
                var medicalRecordId = treatmentPlan.MedicalRecordId;
                _context.TreatmentPlans.Remove(treatmentPlan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageDiagnoses), new { id = medicalRecordId });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MedicalRecordExists(int id)
        {
            return _context.MedicalRecords.Any(e => e.Id == id);
        }
    }
} 