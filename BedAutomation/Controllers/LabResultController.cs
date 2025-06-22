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
    public class LabResultController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LabResultController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LabResult
        public async Task<IActionResult> Index(int? patientId, int? labTestId, string? status)
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName");
            ViewBag.LabTests = new SelectList(await _context.LabTests.ToListAsync(), "Id", "Name");
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" });

            var labResults = _context.LabResults
                .Include(l => l.Patient)
                .Include(l => l.LabTest)
                .Include(l => l.LabParameter)
                .AsQueryable();

            if (patientId.HasValue)
                labResults = labResults.Where(l => l.PatientId == patientId.Value);
            
            if (labTestId.HasValue)
                labResults = labResults.Where(l => l.LabTestId == labTestId.Value);
            
            if (!string.IsNullOrEmpty(status))
                labResults = labResults.Where(l => l.Status == status);

            // Statistics
            var totalResults = await labResults.CountAsync();
            var criticalResults = await labResults.CountAsync(l => l.Status == "Critical");
            var abnormalResults = await labResults.CountAsync(l => l.Status != "Normal");
            var unverifiedResults = await labResults.CountAsync(l => !l.IsVerified);

            ViewBag.TotalResults = totalResults;
            ViewBag.CriticalResults = criticalResults;
            ViewBag.AbnormalResults = abnormalResults;
            ViewBag.UnverifiedResults = unverifiedResults;

            return View(await labResults.OrderByDescending(l => l.TestDate).ToListAsync());
        }

        // GET: LabResult/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var labResult = await _context.LabResults
                .Include(l => l.Patient)
                .Include(l => l.LabTest)
                .Include(l => l.LabParameter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (labResult == null) return NotFound();

            return View(labResult);
        }

        // GET: LabResult/Create
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName");
            ViewBag.LabTests = new SelectList(await _context.LabTests.Where(l => l.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.LabParameters = new SelectList(new List<object>(), "Id", "Name");  // Empty initially, filled by AJAX
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" });
            return View();
        }

        // POST: LabResult/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create(LabResult labResult)
        {
            // Debug received values
            Console.WriteLine($"Received PatientId: {labResult.PatientId}");
            Console.WriteLine($"Received LabTestId: {labResult.LabTestId}");
            Console.WriteLine($"Received LabParameterId: {labResult.LabParameterId}");
            Console.WriteLine($"Received Status: {labResult.Status}");
            
            // Clear default ModelState errors for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("LabTest");
            ModelState.Remove("LabParameter");
            
            // Manual validation for required fields
            if (labResult.PatientId <= 0)
                ModelState.AddModelError("PatientId", "Please select a patient.");
            if (labResult.LabTestId <= 0)
                ModelState.AddModelError("LabTestId", "Please select a lab test.");
            if (labResult.LabParameterId <= 0)
                ModelState.AddModelError("LabParameterId", "Please select a parameter.");
            if (string.IsNullOrEmpty(labResult.Status))
                ModelState.AddModelError("Status", "Please select a status.");
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure UTC dates
                    if (labResult.TestDate.Kind == DateTimeKind.Unspecified)
                        labResult.TestDate = DateTime.SpecifyKind(labResult.TestDate, DateTimeKind.Utc);
                    if (labResult.ResultDate.Kind == DateTimeKind.Unspecified)
                        labResult.ResultDate = DateTime.SpecifyKind(labResult.ResultDate, DateTimeKind.Utc);
                    
                    labResult.CreatedAt = DateTime.UtcNow;
                    _context.Add(labResult);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving LabResult: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving the lab result.");
                }
            }
            
            // If we got here, something failed, redisplay form
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", labResult.PatientId);
            ViewBag.LabTests = new SelectList(await _context.LabTests.Where(l => l.IsActive).ToListAsync(), "Id", "Name", labResult.LabTestId);
            ViewBag.LabParameters = new SelectList(await _context.LabParameters.Where(p => p.LabTestId == labResult.LabTestId).ToListAsync(), "Id", "Name", labResult.LabParameterId);
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" }, labResult.Status);
            return View(labResult);
        }

        // GET: LabResult/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var labResult = await _context.LabResults.FindAsync(id);
            if (labResult == null) return NotFound();

            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", labResult.PatientId);
            ViewBag.LabTests = new SelectList(await _context.LabTests.Where(l => l.IsActive).ToListAsync(), "Id", "Name", labResult.LabTestId);
            ViewBag.LabParameters = new SelectList(await _context.LabParameters.Where(p => p.LabTestId == labResult.LabTestId).ToListAsync(), "Id", "Name", labResult.LabParameterId);
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" }, labResult.Status);
            return View(labResult);
        }

        // POST: LabResult/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, LabResult labResult)
        {
            if (id != labResult.Id) return NotFound();

            // Clear default ModelState errors for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("LabTest");
            ModelState.Remove("LabParameter");

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure UTC dates
                    if (labResult.TestDate.Kind == DateTimeKind.Unspecified)
                        labResult.TestDate = DateTime.SpecifyKind(labResult.TestDate, DateTimeKind.Utc);
                    if (labResult.ResultDate.Kind == DateTimeKind.Unspecified)
                        labResult.ResultDate = DateTime.SpecifyKind(labResult.ResultDate, DateTimeKind.Utc);
                    
                    labResult.UpdatedAt = DateTime.UtcNow;
                    _context.Update(labResult);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabResultExists(labResult.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "FullName", labResult.PatientId);
            ViewBag.LabTests = new SelectList(await _context.LabTests.Where(l => l.IsActive).ToListAsync(), "Id", "Name", labResult.LabTestId);
            ViewBag.LabParameters = new SelectList(await _context.LabParameters.Where(p => p.LabTestId == labResult.LabTestId).ToListAsync(), "Id", "Name", labResult.LabParameterId);
            ViewBag.StatusList = new SelectList(new[] { "Normal", "High", "Low", "Critical" }, labResult.Status);
            return View(labResult);
        }

        // GET: LabResult/Delete/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var labResult = await _context.LabResults
                .Include(l => l.Patient)
                .Include(l => l.LabTest)
                .Include(l => l.LabParameter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (labResult == null) return NotFound();

            return View(labResult);
        }

        // POST: LabResult/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var labResult = await _context.LabResults.FindAsync(id);
            if (labResult != null)
            {
                _context.LabResults.Remove(labResult);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get Lab Parameters by Lab Test
        [HttpGet]
        public async Task<JsonResult> GetLabParameters(int labTestId)
        {
            var parameters = await _context.LabParameters
                .Where(p => p.LabTestId == labTestId)
                .Select(p => new { Id = p.Id, Name = p.Name, Unit = p.Unit })
                .ToListAsync();
            
            return Json(parameters);
        }

        private bool LabResultExists(int id)
        {
            return _context.LabResults.Any(e => e.Id == id);
        }
    }
} 