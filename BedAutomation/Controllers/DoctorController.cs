using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BedAutomation.Data;
using BedAutomation.Models;

namespace BedAutomation.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DoctorController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Doctor
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Reservations)
                .OrderBy(d => d.FirstName)
                .ToListAsync();

            return View(doctors);
        }

        // GET: Doctor/Details/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(m => m.Id == id);

            if (doctor == null) return NotFound();

            // Check if current user is the doctor or admin
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }
            
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            
            if (!userRoles.Contains("Admin") && doctor.UserId != currentUser.Id)
            {
                return Forbid();
            }

            return View(doctor);
        }

        // GET: Doctor/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Doctor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,IdentityNumber,Email,PhoneNumber,LicenseNumber,Specialization,YearsOfExperience,Biography,IsActive")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate identity number
                    var existingDoctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.IdentityNumber == doctor.IdentityNumber);
                    
                    if (existingDoctor != null)
                    {
                        ModelState.AddModelError("IdentityNumber", "A doctor with this identity number already exists.");
                        return View(doctor);
                    }

                    // Check for duplicate license number
                    var existingLicense = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.LicenseNumber == doctor.LicenseNumber);
                    
                    if (existingLicense != null)
                    {
                        ModelState.AddModelError("LicenseNumber", "A doctor with this license number already exists.");
                        return View(doctor);
                    }

                    doctor.CreatedAt = DateTime.UtcNow;
                    _context.Add(doctor);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Doctor created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists contact your system administrator.");
                }
            }
            return View(doctor);
        }

        // GET: Doctor/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            // Check if current user is the doctor or admin
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            
            if (!userRoles.Contains("Admin") && doctor.UserId != currentUser.Id)
            {
                return Forbid();
            }

            return View(doctor);
        }

        // POST: Doctor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,IdentityNumber,Email,PhoneNumber,LicenseNumber,Specialization,YearsOfExperience,Biography,IsActive,CreatedAt")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            // Check if current user is the doctor or admin
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            
            if (!userRoles.Contains("Admin") && doctor.UserId != currentUser.Id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    doctor.UpdatedAt = DateTime.UtcNow;
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Doctor updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists contact your system administrator.");
                }
            }
            return View(doctor);
        }

        // GET: Doctor/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Reservations)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Reservations)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (doctor != null)
            {
                // Check if doctor has active reservations
                var activeReservations = doctor.Reservations.Any(r => r.Status == "Active" || r.Status == "Reserved");
                
                if (activeReservations)
                {
                    TempData["ErrorMessage"] = "Cannot delete doctor with active reservations. Please reassign or complete reservations first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Doctor deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Doctor/CreateProfile
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // Check if doctor profile already exists
            var existingDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (existingDoctor != null)
            {
                return RedirectToAction("MyProfile");
            }

            // Pre-fill with user information
            var model = new Doctor
            {
                Email = currentUser.Email ?? "",
                UserId = currentUser.Id
            };

            return View(model);
        }

        // POST: Doctor/CreateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateProfile([Bind("FirstName,LastName,IdentityNumber,Email,PhoneNumber,LicenseNumber,Specialization,YearsOfExperience,Biography")] Doctor doctor)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // Check if doctor profile already exists
            var existingDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (existingDoctor != null)
            {
                return RedirectToAction("MyProfile");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate identity number
                    var duplicateIdentity = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.IdentityNumber == doctor.IdentityNumber);
                    
                    if (duplicateIdentity != null)
                    {
                        ModelState.AddModelError("IdentityNumber", "A doctor with this identity number already exists.");
                        return View(doctor);
                    }

                    // Check for duplicate license number
                    var duplicateLicense = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.LicenseNumber == doctor.LicenseNumber);
                    
                    if (duplicateLicense != null)
                    {
                        ModelState.AddModelError("LicenseNumber", "A doctor with this license number already exists.");
                        return View(doctor);
                    }

                    // Set additional properties
                    doctor.UserId = currentUser.Id;
                    doctor.IsActive = true;
                    doctor.CreatedAt = DateTime.UtcNow;

                    _context.Add(doctor);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Doctor profile created successfully!";
                    return RedirectToAction("MyProfile");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists contact your system administrator.");
                }
            }

            return View(doctor);
        }

        // GET: Doctor/MyProfile
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .Include(d => d.Reservations)
                    .ThenInclude(r => r.Patient)
                .Include(d => d.Reservations)
                    .ThenInclude(r => r.Bed)
                        .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                // If no doctor profile exists, redirect to create one
                return RedirectToAction("CreateProfile");
            }

            return View(doctor);
        }

        // GET: Doctor/MyReservations
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MyReservations()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUser.Id);

            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor profile not found. Please create your profile first.";
                return RedirectToAction("CreateProfile");
            }

            var reservations = await _context.Reservations
                .Include(r => r.Patient)
                .Include(r => r.Bed)
                    .ThenInclude(b => b.Room)
                .Where(r => r.DoctorId == doctor.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reservations);
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(d => d.Id == id);
        }
    }
} 