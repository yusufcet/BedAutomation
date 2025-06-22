using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using BedAutomation.Data;
using BedAutomation.Models;

namespace BedAutomation.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReservationController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservation (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int? patientId, int? doctorId, string? status, string? department)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles == null || !roles.Any())
            {
                return Challenge();
            }

            var reservations = await _context.Reservations
                .Include(r => r.Patient)
                .Include(r => r.Bed)
                    .ThenInclude(b => b.Room)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reservations);
        }

        // GET: Reservation/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Patient)
                .Include(r => r.Bed)
                    .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservation/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            var model = new Reservation();

            if (roles.Contains("Patient"))
            {
                // For patients, automatically use their profile
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Please create your patient profile first.";
                    return RedirectToAction("CreateProfile", "Patient");
                }
                
                // Check if patient has an active reservation
                var hasActiveReservation = await _context.Reservations
                    .AnyAsync(r => r.PatientId == patient.Id && 
                                  (r.Status == "Reserved" || r.Status == "Active"));
                
                if (hasActiveReservation)
                {
                    TempData["ErrorMessage"] = "You already have an active reservation. Please complete or cancel it first.";
                    return RedirectToAction("MyReservations", "Patient");
                }
                
                model.PatientId = patient.Id;
                ViewBag.IsPatientUser = true;
                ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";
            }
            else
            {
                // For admin, show all patients
                ViewBag.IsPatientUser = false;
                ViewBag.Patients = await _context.Patients
                    .Select(p => new SelectListItem 
                    { 
                        Value = p.Id.ToString(), 
                        Text = $"{p.FirstName} {p.LastName} - {p.IdentityNumber}" 
                    })
                    .ToListAsync();
            }

            // Load departments for cascade dropdowns
            ViewBag.Departments = await _context.Rooms
                .Where(r => r.IsActive)
                .Select(r => r.Department)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => new SelectListItem { Value = d, Text = d })
                .ToListAsync();

            // Load doctors for assignment
            ViewBag.Doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .Select(d => new SelectListItem 
                { 
                    Value = d.Id.ToString(), 
                    Text = $"Dr. {d.FirstName} {d.LastName} - {d.Specialization}" 
                })
                .ToListAsync();

            return View(model);
        }

                // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation model)
        {
            // Debug: Log that POST method is called
            Console.WriteLine("=== CREATE POST METHOD CALLED ===");
            Console.WriteLine($"Received PatientId: {model.PatientId}");
            Console.WriteLine($"Received BedId: {model.BedId}");
            Console.WriteLine($"Received CheckInDate: {model.CheckInDate}");
            Console.WriteLine($"Received Priority: {model.Priority}");
            Console.WriteLine($"Received AdmissionType: {model.AdmissionType}");
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                Console.WriteLine("ERROR: Current user is null");
                return Challenge();
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            Console.WriteLine($"User roles: {string.Join(", ", roles)}");
            
            // For Patient users, ensure PatientId is set correctly
            if (roles.Contains("Patient"))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient profile not found.";
                    return RedirectToAction("CreateProfile", "Patient");
                }
                model.PatientId = patient.Id;
            }

            // Clear ModelState and do manual validation since values are coming correctly
            ModelState.Clear();
            
            // Manual validation
            Console.WriteLine($"Final validation - PatientId: {model.PatientId}, BedId: {model.BedId}");
            
            if (model.PatientId <= 0)
            {
                ModelState.AddModelError("PatientId", "Patient selection is required.");
            }

            if (model.BedId <= 0)
            {
                ModelState.AddModelError("BedId", "Bed selection is required.");
            }

            if (model.CheckInDate == default(DateTime))
            {
                ModelState.AddModelError("CheckInDate", "Check-in date is required.");
            }

            // Check if patient already has an active reservation
            if (model.PatientId > 0)
            {
                var hasActiveReservation = await _context.Reservations
                    .AnyAsync(r => r.PatientId == model.PatientId && 
                                  (r.Status == "Reserved" || r.Status == "Active"));
                
                if (hasActiveReservation)
                {
                    ModelState.AddModelError("", "This patient already has an active reservation.");
                }
            }

            // Check if bed is available
            if (model.BedId > 0)
            {
                var bed = await _context.Beds.FindAsync(model.BedId);
                if (bed == null || bed.IsOccupied || !bed.IsActive)
                {
                    ModelState.AddModelError("BedId", "Selected bed is not available.");
                }
            }

            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODEL STATE ERRORS ===");
                foreach (var modelError in ModelState)
                {
                    var key = modelError.Key;
                    var errors = modelError.Value.Errors;
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($"{key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("=== ATTEMPTING TO SAVE TO DATABASE ===");
                try
                {
                    // Set reservation properties
                    model.Status = "Reserved";
                    model.CreatedAt = DateTime.UtcNow;
                    model.UpdatedAt = DateTime.UtcNow;
                    
                    // Ensure dates are in UTC
                    if (model.CheckInDate.Kind == DateTimeKind.Unspecified)
                    {
                        model.CheckInDate = DateTime.SpecifyKind(model.CheckInDate, DateTimeKind.Utc);
                    }
                    if (model.CheckOutDate.HasValue && model.CheckOutDate.Value.Kind == DateTimeKind.Unspecified)
                    {
                        model.CheckOutDate = DateTime.SpecifyKind(model.CheckOutDate.Value, DateTimeKind.Utc);
                    }
                    
                    // Add reservation
                    _context.Reservations.Add(model);
                    
                    // Update bed status
                    var bed = await _context.Beds.FindAsync(model.BedId);
                    if (bed != null)
                    {
                        bed.IsOccupied = true;
                        _context.Beds.Update(bed);
                    }
                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine("=== DATABASE SAVE SUCCESSFUL ===");
                    
                    TempData["SuccessMessage"] = "Reservation created successfully!";
                    
                    if (roles.Contains("Patient"))
                    {
                        return RedirectToAction("MyReservations", "Patient");
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"=== DATABASE ERROR ===");
                    Console.WriteLine($"Exception: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"An error occurred while creating the reservation: {ex.Message}");
                }
            }

            // Reload data for form in case of error
            if (roles.Contains("Patient"))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
                ViewBag.IsPatientUser = true;
                ViewBag.PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "";
            }
            else
            {
                ViewBag.IsPatientUser = false;
                ViewBag.Patients = await _context.Patients
                    .Select(p => new SelectListItem 
                    { 
                        Value = p.Id.ToString(), 
                        Text = $"{p.FirstName} {p.LastName} - {p.IdentityNumber}",
                        Selected = p.Id == model.PatientId
                    })
                    .ToListAsync();
            }

            // Reload departments for cascade dropdowns
            ViewBag.Departments = await _context.Rooms
                .Where(r => r.IsActive)
                .Select(r => r.Department)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => new SelectListItem { Value = d, Text = d })
                .ToListAsync();

            // Reload doctors for assignment
            ViewBag.Doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .Select(d => new SelectListItem 
                { 
                    Value = d.Id.ToString(), 
                    Text = $"Dr. {d.FirstName} {d.LastName} - {d.Specialization}",
                    Selected = d.Id == model.DoctorId
                })
                .ToListAsync();

            return View(model);
        }

        // GET: Reservation/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // For patient dropdown - show full name
            ViewData["PatientId"] = new SelectList(
                await _context.Patients.Select(p => new { p.Id, FullName = $"{p.FirstName} {p.LastName} - {p.IdentityNumber}" }).ToListAsync(), 
                "Id", "FullName", reservation.PatientId);

            // For bed dropdown - include current bed + available beds
            var availableBeds = await GetAvailableBeds();
            var currentBed = await _context.Beds
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == reservation.BedId);
            
            // Combine current bed with available beds if current bed is not in available list
            var bedOptions = availableBeds.ToList();
            if (currentBed != null && !bedOptions.Any(b => b.Id == currentBed.Id))
            {
                bedOptions.Insert(0, currentBed);
            }
            
            ViewData["BedId"] = new SelectList(
                bedOptions.Select(b => new { b.Id, BedInfo = $"Room {b.Room.RoomNumber} - Bed {b.BedNumber} ({b.Room.Department})" }).ToList(),
                "Id", "BedInfo", reservation.BedId);

            ViewData["DoctorId"] = new SelectList(
                await _context.Doctors.Where(d => d.IsActive)
                    .Select(d => new { d.Id, FullName = $"Dr. {d.FirstName} {d.LastName} - {d.Specialization}" })
                    .ToListAsync(), 
                "Id", "FullName", reservation.DoctorId);
            return View(reservation);
        }

        // POST: Reservation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,BedId,CheckInDate,CheckOutDate,Status,Priority,AdmissionType,MedicalNotes,DoctorId")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingReservation = await _context.Reservations.FindAsync(id);
                    if (existingReservation == null)
                    {
                        return NotFound();
                    }

                    // Handle bed change
                    var oldBedId = existingReservation.BedId;
                    var newBedId = reservation.BedId;
                    
                    if (oldBedId != newBedId)
                    {
                        // Free up old bed
                        var oldBed = await _context.Beds.FindAsync(oldBedId);
                        if (oldBed != null)
                        {
                            oldBed.IsOccupied = false;
                            _context.Update(oldBed);
                        }
                        
                        // Occupy new bed
                        var newBed = await _context.Beds.FindAsync(newBedId);
                        if (newBed != null)
                        {
                            newBed.IsOccupied = true;
                            _context.Update(newBed);
                        }
                    }

                    // Update properties
                    existingReservation.PatientId = reservation.PatientId;
                    existingReservation.BedId = reservation.BedId;
                    
                    // Convert DateTime values to UTC for PostgreSQL compatibility
                    if (reservation.CheckInDate.Kind == DateTimeKind.Unspecified)
                    {
                        existingReservation.CheckInDate = DateTime.SpecifyKind(reservation.CheckInDate, DateTimeKind.Utc);
                    }
                    else
                    {
                        existingReservation.CheckInDate = reservation.CheckInDate.ToUniversalTime();
                    }
                    
                    if (reservation.CheckOutDate.HasValue)
                    {
                        if (reservation.CheckOutDate.Value.Kind == DateTimeKind.Unspecified)
                        {
                            existingReservation.CheckOutDate = DateTime.SpecifyKind(reservation.CheckOutDate.Value, DateTimeKind.Utc);
                        }
                        else
                        {
                            existingReservation.CheckOutDate = reservation.CheckOutDate.Value.ToUniversalTime();
                        }
                    }
                    else
                    {
                        existingReservation.CheckOutDate = null;
                    }
                    
                    existingReservation.Status = reservation.Status;
                    existingReservation.Priority = reservation.Priority;
                    existingReservation.AdmissionType = reservation.AdmissionType;
                    existingReservation.MedicalNotes = reservation.MedicalNotes;
                    existingReservation.CancellationReason = reservation.CancellationReason;
                    existingReservation.DoctorId = reservation.DoctorId;
                    existingReservation.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingReservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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

            // For patient dropdown - show full name
            ViewData["PatientId"] = new SelectList(
                await _context.Patients.Select(p => new { p.Id, FullName = $"{p.FirstName} {p.LastName} - {p.IdentityNumber}" }).ToListAsync(), 
                "Id", "FullName", reservation.PatientId);

            // For bed dropdown - include current bed + available beds
            var availableBeds = await GetAvailableBeds();
            var currentBed = await _context.Beds
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == reservation.BedId);
            
            // Combine current bed with available beds if current bed is not in available list
            var bedOptions = availableBeds.ToList();
            if (currentBed != null && !bedOptions.Any(b => b.Id == currentBed.Id))
            {
                bedOptions.Insert(0, currentBed);
            }
            
            ViewData["BedId"] = new SelectList(
                bedOptions.Select(b => new { b.Id, BedInfo = $"Room {b.Room.RoomNumber} - Bed {b.BedNumber} ({b.Room.Department})" }).ToList(),
                "Id", "BedInfo", reservation.BedId);

            ViewData["DoctorId"] = new SelectList(
                await _context.Doctors.Where(d => d.IsActive)
                    .Select(d => new { d.Id, FullName = $"Dr. {d.FirstName} {d.LastName} - {d.Specialization}" })
                    .ToListAsync(), 
                "Id", "FullName", reservation.DoctorId);
            return View(reservation);
        }

        // POST: Reservation/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Bed)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = "Completed";
            reservation.CheckOutDate = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            // Make bed available
            reservation.Bed.IsOccupied = false;

            _context.Update(reservation);
            _context.Update(reservation.Bed);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Reservation/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string cancellationReason)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Bed)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = "Cancelled";
            reservation.CancellationReason = cancellationReason;
            reservation.UpdatedAt = DateTime.UtcNow;

            // Make bed available
            reservation.Bed.IsOccupied = false;

            _context.Update(reservation);
            _context.Update(reservation.Bed);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Available beds
        private async Task<List<Bed>> GetAvailableBeds()
        {
            return await _context.Beds
                .Include(b => b.Room)
                .Where(b => !b.IsOccupied && b.IsActive && !b.IsMaintenanceRequired)
                .ToListAsync();
        }

        // API endpoints for cascade dropdowns
        [HttpGet]
        public async Task<JsonResult> GetRoomTypes(string department)
        {
            var roomTypes = await _context.Rooms
                .Where(r => r.Department == department && r.IsActive)
                .Select(r => r.RoomType)
                .Distinct()
                .OrderBy(rt => rt)
                .Select(rt => new { Value = rt, Text = rt })
                .ToListAsync();

            return Json(roomTypes);
        }

        [HttpGet]
        public async Task<JsonResult> GetRooms(string department, string roomType)
        {
            var rooms = await _context.Rooms
                .Where(r => r.Department == department && r.RoomType == roomType && r.IsActive)
                .OrderBy(r => r.RoomNumber)
                .Select(r => new { Value = r.Id, Text = $"Room {r.RoomNumber} (Floor {r.Floor})" })
                .ToListAsync();

            return Json(rooms);
        }

        [HttpGet]
        public async Task<JsonResult> GetBeds(int roomId)
        {
            var beds = await _context.Beds
                .Include(b => b.Room)
                .Where(b => b.RoomId == roomId && b.IsActive && !b.IsOccupied && !b.IsMaintenanceRequired)
                .OrderBy(b => b.BedNumber)
                .Select(b => new { Value = b.Id, Text = $"Bed {b.BedNumber} ({b.BedType})" })
                .ToListAsync();

            return Json(beds);
        }

        // GET: Reservation/Calendar
        [Authorize]
        public async Task<IActionResult> Calendar()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Patient)
                .Include(r => r.Bed)
                    .ThenInclude(b => b.Room)
                .Include(r => r.Doctor)
                .OrderBy(r => r.CheckInDate)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Reservation/GetCalendarEvents (API endpoint for FullCalendar)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Patient)
                .Include(r => r.Bed)
                    .ThenInclude(b => b.Room)
                .Include(r => r.Doctor)
                .Select(r => new
                {
                    id = r.Id,
                                         title = $"{r.Patient.FirstName} {r.Patient.LastName} - Room {r.Bed.Room.RoomNumber}",
                    start = r.CheckInDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = r.CheckOutDate.HasValue ? r.CheckOutDate.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    backgroundColor = r.Status == "Active" ? "#28a745" : 
                                    r.Status == "Reserved" ? "#007bff" : 
                                    r.Status == "Completed" ? "#6c757d" : "#dc3545",
                    borderColor = r.Status == "Active" ? "#28a745" : 
                                r.Status == "Reserved" ? "#007bff" : 
                                r.Status == "Completed" ? "#6c757d" : "#dc3545",
                    extendedProps = new
                    {
                                                 patient = $"{r.Patient.FirstName} {r.Patient.LastName}",
                        room = $"Room {r.Bed.Room.RoomNumber}",
                        bed = $"Bed {r.Bed.BedNumber}",
                        doctor = r.Doctor != null ? r.Doctor.DisplayName : "Not assigned",
                        status = r.Status,
                        priority = r.Priority,
                        admissionType = r.AdmissionType
                    }
                })
                .ToListAsync();

            return Json(reservations);
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
} 