using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using BedAutomation.Data;
using BedAutomation.Models;

namespace BedAutomation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BedController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bed
        public async Task<IActionResult> Index()
        {
            var beds = await _context.Beds
                .Include(b => b.Room)
                .Include(b => b.Reservations.Where(r => r.Status == "Active" || r.Status == "Reserved"))
                    .ThenInclude(r => r.Patient)
                .OrderBy(b => b.Room.RoomNumber)
                .ThenBy(b => b.BedNumber)
                .ToListAsync();
            return View(beds);
        }

        // GET: Bed/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var bed = await _context.Beds
                .Include(b => b.Room)
                .Include(b => b.Reservations)
                    .ThenInclude(r => r.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bed == null)
            {
                return NotFound();
            }

            return View(bed);
        }

        // GET: Bed/Create
        public async Task<IActionResult> Create()
        {
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive).ToListAsync(), "Id", "RoomNumber");
            return View();
        }

        // POST: Bed/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BedNumber,RoomId,BedType,Notes")] Bed bed)
        {
            if (ModelState.IsValid)
            {
                bed.IsOccupied = false;
                bed.IsActive = true;
                bed.IsMaintenanceRequired = false;
                
                _context.Add(bed);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive).ToListAsync(), "Id", "RoomNumber", bed.RoomId);
            return View(bed);
        }

        // GET: Bed/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null)
            {
                return NotFound();
            }
            
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive).ToListAsync(), "Id", "RoomNumber", bed.RoomId);
            return View(bed);
        }

        // POST: Bed/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BedNumber,RoomId,BedType,IsActive,IsMaintenanceRequired,Notes")] Bed bed)
        {
            if (id != bed.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBed = await _context.Beds.FindAsync(id);
                    if (existingBed == null)
                    {
                        return NotFound();
                    }

                    existingBed.BedNumber = bed.BedNumber;
                    existingBed.RoomId = bed.RoomId;
                    existingBed.BedType = bed.BedType;
                    existingBed.IsActive = bed.IsActive;
                    existingBed.IsMaintenanceRequired = bed.IsMaintenanceRequired;
                    existingBed.Notes = bed.Notes;

                    _context.Update(existingBed);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BedExists(bed.Id))
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
            
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive).ToListAsync(), "Id", "RoomNumber", bed.RoomId);
            return View(bed);
        }

        // POST: Bed/ToggleMaintenance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMaintenance(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null)
            {
                return NotFound();
            }

            bed.IsMaintenanceRequired = !bed.IsMaintenanceRequired;
            if (bed.IsMaintenanceRequired)
            {
                bed.IsOccupied = false; // Free the bed if it goes to maintenance
            }

            _context.Update(bed);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Bed/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var bed = await _context.Beds
                .Include(b => b.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bed == null)
            {
                return NotFound();
            }

            return View(bed);
        }

        // POST: Bed/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed != null)
            {
                _context.Beds.Remove(bed);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BedExists(int id)
        {
            return _context.Beds.Any(e => e.Id == id);
        }
    }
} 