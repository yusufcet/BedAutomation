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
    public class VitalSignController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VitalSignController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: VitalSign
        public async Task<IActionResult> Index(int? patientId, int? vitalSignTypeId, string? status)
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName");
            ViewBag.VitalSignTypes = new SelectList(await _context.VitalSignTypes.Where(v => v.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" });

            var vitalSigns = _context.VitalSigns
                .Include(v => v.Patient)
                .Include(v => v.VitalSignType)
                .AsQueryable();

            if (patientId.HasValue)
                vitalSigns = vitalSigns.Where(v => v.PatientId == patientId.Value);
            
            if (vitalSignTypeId.HasValue)
                vitalSigns = vitalSigns.Where(v => v.VitalSignTypeId == vitalSignTypeId.Value);
            
            if (!string.IsNullOrEmpty(status))
                vitalSigns = vitalSigns.Where(v => v.Status == status);

            // Statistics
            var totalSigns = await vitalSigns.CountAsync();
            var criticalSigns = await vitalSigns.CountAsync(v => v.Status == "Critical");
            var abnormalSigns = await vitalSigns.CountAsync(v => v.Status != "Normal");
            var unverifiedSigns = await vitalSigns.CountAsync(v => !v.IsVerified);

            ViewBag.TotalSigns = totalSigns;
            ViewBag.CriticalSigns = criticalSigns;
            ViewBag.AbnormalSigns = abnormalSigns;
            ViewBag.UnverifiedSigns = unverifiedSigns;

            return View(await vitalSigns.OrderByDescending(v => v.MeasurementDate).ToListAsync());
        }

        // GET: VitalSign/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vitalSign = await _context.VitalSigns
                .Include(v => v.Patient)
                .Include(v => v.VitalSignType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vitalSign == null) return NotFound();

            return View(vitalSign);
        }

        // GET: VitalSign/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName");
            ViewBag.VitalSignTypes = new SelectList(await _context.VitalSignTypes.Where(v => v.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" });
            return View();
        }

        // POST: VitalSign/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VitalSign vitalSign)
        {
            // Clear default ModelState errors for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("VitalSignType");
            
            // Manual validation for required fields
            if (vitalSign.PatientId <= 0)
                ModelState.AddModelError("PatientId", "Please select a patient.");
            if (vitalSign.VitalSignTypeId <= 0)
                ModelState.AddModelError("VitalSignTypeId", "Please select a vital sign type.");
            if (string.IsNullOrEmpty(vitalSign.Status))
                ModelState.AddModelError("Status", "Please select a status.");
            
            if (ModelState.IsValid)
            {
                // Ensure UTC dates
                if (vitalSign.MeasurementDate.Kind == DateTimeKind.Unspecified)
                    vitalSign.MeasurementDate = DateTime.SpecifyKind(vitalSign.MeasurementDate, DateTimeKind.Utc);
                
                vitalSign.CreatedAt = DateTime.UtcNow;
                _context.Add(vitalSign);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", vitalSign.PatientId);
            ViewBag.VitalSignTypes = new SelectList(await _context.VitalSignTypes.Where(v => v.IsActive).ToListAsync(), "Id", "Name", vitalSign.VitalSignTypeId);
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" }, vitalSign.Status);
            return View(vitalSign);
        }

        // GET: VitalSign/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vitalSign = await _context.VitalSigns.FindAsync(id);
            if (vitalSign == null) return NotFound();

            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", vitalSign.PatientId);
            ViewBag.VitalSignTypes = new SelectList(await _context.VitalSignTypes.Where(v => v.IsActive).ToListAsync(), "Id", "Name", vitalSign.VitalSignTypeId);
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" }, vitalSign.Status);
            return View(vitalSign);
        }

        // POST: VitalSign/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VitalSign vitalSign)
        {
            if (id != vitalSign.Id) return NotFound();

            // Clear default ModelState errors for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("VitalSignType");

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure UTC dates
                    if (vitalSign.MeasurementDate.Kind == DateTimeKind.Unspecified)
                        vitalSign.MeasurementDate = DateTime.SpecifyKind(vitalSign.MeasurementDate, DateTimeKind.Utc);
                    
                    vitalSign.UpdatedAt = DateTime.UtcNow;
                    _context.Update(vitalSign);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VitalSignExists(vitalSign.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", vitalSign.PatientId);
            ViewBag.VitalSignTypes = new SelectList(await _context.VitalSignTypes.Where(v => v.IsActive).ToListAsync(), "Id", "Name", vitalSign.VitalSignTypeId);
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" }, vitalSign.Status);
            return View(vitalSign);
        }

        // GET: VitalSign/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vitalSign = await _context.VitalSigns
                .Include(v => v.Patient)
                .Include(v => v.VitalSignType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vitalSign == null) return NotFound();

            return View(vitalSign);
        }

        // POST: VitalSign/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vitalSign = await _context.VitalSigns.FindAsync(id);
            if (vitalSign != null)
            {
                _context.VitalSigns.Remove(vitalSign);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: VitalSign/Chart/5
        public async Task<IActionResult> Chart(int? patientId, int? vitalSignTypeId)
        {
            if (patientId == null || vitalSignTypeId == null) return NotFound();

            var patient = await _context.Patients.FindAsync(patientId);
            var vitalSignType = await _context.VitalSignTypes.FindAsync(vitalSignTypeId);
            
            if (patient == null || vitalSignType == null) return NotFound();

            var vitalSigns = await _context.VitalSigns
                .Where(v => v.PatientId == patientId && v.VitalSignTypeId == vitalSignTypeId)
                .OrderBy(v => v.MeasurementDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            ViewBag.VitalSignType = vitalSignType;
            ViewBag.ChartData = vitalSigns.Select(v => new { 
                Date = v.MeasurementDate.ToString("yyyy-MM-dd HH:mm"), 
                Value = v.Value,
                Status = v.Status 
            }).ToArray();

            return View(vitalSigns);
        }

        private bool VitalSignExists(int id)
        {
            return _context.VitalSigns.Any(e => e.Id == id);
        }
    }
} 