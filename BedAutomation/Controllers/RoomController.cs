using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BedAutomation.Data;
using BedAutomation.Models;

namespace BedAutomation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Room
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Beds)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
            return View(rooms);
        }

        // GET: Room/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Beds)
                    .ThenInclude(b => b.Reservations)
                        .ThenInclude(res => res.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Room/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomNumber,RoomType,Department,Floor,BedCapacity,Description")] Room room)
        {
            if (ModelState.IsValid)
            {
                room.IsActive = true;
                _context.Add(room);
                await _context.SaveChangesAsync();
                
                // Create beds for the room
                for (int i = 1; i <= room.BedCapacity; i++)
                {
                    var bed = new Bed
                    {
                        BedNumber = $"{room.RoomNumber}-{i:D2}",
                        RoomId = room.Id,
                        BedType = "Standard",
                        IsOccupied = false,
                        IsActive = true,
                        IsMaintenanceRequired = false
                    };
                    _context.Beds.Add(bed);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Room/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Room/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomNumber,RoomType,Department,Floor,BedCapacity,IsActive,Description")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoom = await _context.Rooms.FindAsync(id);
                    if (existingRoom == null)
                    {
                        return NotFound();
                    }

                    existingRoom.RoomNumber = room.RoomNumber;
                    existingRoom.RoomType = room.RoomType;
                    existingRoom.Department = room.Department;
                    existingRoom.Floor = room.Floor;
                    existingRoom.BedCapacity = room.BedCapacity;
                    existingRoom.IsActive = room.IsActive;
                    existingRoom.Description = room.Description;

                    _context.Update(existingRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
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
            return View(room);
        }

        // GET: Room/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Beds)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
} 