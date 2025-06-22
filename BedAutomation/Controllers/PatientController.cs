using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BedAutomation.Data;
using BedAutomation.Models;
using Microsoft.AspNetCore.Identity;

namespace BedAutomation.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patient (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Reservations)
                .ToListAsync();
            return View(patients);
        }

        // GET: Patient/MyReservations (Patient role)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyReservations()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .Include(p => p.Reservations)
                    .ThenInclude(r => r.Bed)
                        .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                // If patient profile doesn't exist, redirect to create profile
                return RedirectToAction("CreateProfile");
            }

            ViewBag.Patient = patient;
            return View(patient.Reservations.OrderByDescending(r => r.CreatedAt).ToList());
        }

        // GET: Patient/CreateProfile (Patient role)
        [Authorize(Roles = "Patient")]
        public IActionResult CreateProfile()
        {
            return View();
        }

        // POST: Patient/CreateProfile (Patient role)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CreateProfile([Bind("FirstName,LastName,IdentityNumber,PhoneNumber,Email,DateOfBirth,Gender,Address")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }
                
                patient.UserId = currentUser.Id;
                patient.CreatedAt = DateTime.UtcNow;
                
                // Convert DateOfBirth to UTC if necessary
                if (patient.DateOfBirth.Kind == DateTimeKind.Unspecified)
                {
                    patient.DateOfBirth = DateTime.SpecifyKind(patient.DateOfBirth, DateTimeKind.Utc);
                }
                
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction("MyReservations");
            }
            return View(patient);
        }

        // GET: Patient/MyProfile (Patient role)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return RedirectToAction("CreateProfile");
            }

            return View(patient);
        }

        // GET: Patient/EditProfile (Patient role)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> EditProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return RedirectToAction("CreateProfile");
            }

            return View(patient);
        }

        // POST: Patient/EditProfile (Patient role)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> EditProfile([Bind("Id,FirstName,LastName,IdentityNumber,PhoneNumber,Email,DateOfBirth,Gender,Address")] Patient patient)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (existingPatient == null || existingPatient.Id != patient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingPatient.FirstName = patient.FirstName;
                    existingPatient.LastName = patient.LastName;
                    existingPatient.IdentityNumber = patient.IdentityNumber;
                    existingPatient.PhoneNumber = patient.PhoneNumber;
                    existingPatient.Email = patient.Email;
                    existingPatient.DateOfBirth = patient.DateOfBirth;
                    existingPatient.Gender = patient.Gender;
                    existingPatient.Address = patient.Address;

                    _context.Update(existingPatient);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("MyProfile");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(patient);
        }

        // Admin-only methods
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Reservations)
                    .ThenInclude(r => r.Bed)
                        .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,IdentityNumber,PhoneNumber,Email,DateOfBirth,Gender,Address")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                patient.UserId = currentUser?.Id;
                patient.CreatedAt = DateTime.UtcNow;
                
                // Convert DateOfBirth to UTC if necessary
                if (patient.DateOfBirth.Kind == DateTimeKind.Unspecified)
                {
                    patient.DateOfBirth = DateTime.SpecifyKind(patient.DateOfBirth, DateTimeKind.Utc);
                }
                
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,IdentityNumber,PhoneNumber,Email,DateOfBirth,Gender,Address")] Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPatient = await _context.Patients.FindAsync(id);
                    if (existingPatient == null)
                    {
                        return NotFound();
                    }

                    existingPatient.FirstName = patient.FirstName;
                    existingPatient.LastName = patient.LastName;
                    existingPatient.IdentityNumber = patient.IdentityNumber;
                    existingPatient.PhoneNumber = patient.PhoneNumber;
                    existingPatient.Email = patient.Email;
                    existingPatient.DateOfBirth = patient.DateOfBirth;
                    existingPatient.Gender = patient.Gender;
                    existingPatient.Address = patient.Address;

                    _context.Update(existingPatient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Patient/MyLabResults (Patient role)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyLabResults()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return RedirectToAction("CreateProfile");
            }

            var labResults = await _context.LabResults
                .Include(l => l.LabTest)
                .Include(l => l.LabParameter)
                .Where(l => l.PatientId == patient.Id)
                .OrderByDescending(l => l.TestDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            ViewBag.TotalResults = labResults.Count;
            ViewBag.CriticalResults = labResults.Count(l => l.Status == "Critical");
            ViewBag.AbnormalResults = labResults.Count(l => l.Status != "Normal");
            ViewBag.RecentResults = labResults.Count(l => l.TestDate >= DateTime.UtcNow.AddDays(-30));

            return View(labResults);
        }

        // GET: Patient/MyVitalSigns (Patient role)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyVitalSigns()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return RedirectToAction("CreateProfile");
            }

            var vitalSigns = await _context.VitalSigns
                .Include(v => v.VitalSignType)
                .Where(v => v.PatientId == patient.Id)
                .OrderByDescending(v => v.MeasurementDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            ViewBag.TotalReadings = vitalSigns.Count;
            ViewBag.CriticalReadings = vitalSigns.Count(v => v.Status == "Critical");
            ViewBag.AbnormalReadings = vitalSigns.Count(v => v.Status != "Normal");
            ViewBag.RecentReadings = vitalSigns.Count(v => v.MeasurementDate >= DateTime.UtcNow.AddDays(-7));

            return View(vitalSigns);
        }

        // GET: Patient/MyPrescriptions (Patient role)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyPrescriptions()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return RedirectToAction("CreateProfile");
            }

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionMedications)
                    .ThenInclude(pm => pm.Medication)
                .Where(p => p.PatientId == patient.Id)
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            ViewBag.TotalPrescriptions = prescriptions.Count;
            ViewBag.ActivePrescriptions = prescriptions.Count(p => p.Status == "Active");
            ViewBag.CompletedPrescriptions = prescriptions.Count(p => p.Status == "Completed");
            ViewBag.RecentPrescriptions = prescriptions.Count(p => p.PrescriptionDate >= DateTime.UtcNow.AddDays(-30));

            return View(prescriptions);
        }

        // GET: Patient/MyMedicalRecords (Patient role)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyMedicalRecords()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (patient == null)
            {
                return RedirectToAction("CreateProfile");
            }

            var medicalRecords = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Diagnoses)
                .Include(m => m.TreatmentPlans)
                .Where(m => m.PatientId == patient.Id)
                .OrderByDescending(m => m.RecordDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            ViewBag.TotalRecords = medicalRecords.Count;
            ViewBag.RecentRecords = medicalRecords.Count(m => m.RecordDate >= DateTime.UtcNow.AddDays(-30));
            ViewBag.DiagnosisCount = medicalRecords.SelectMany(m => m.Diagnoses).Count();
            ViewBag.TreatmentCount = medicalRecords.SelectMany(m => m.TreatmentPlans).Count();

            return View(medicalRecords);
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
} 