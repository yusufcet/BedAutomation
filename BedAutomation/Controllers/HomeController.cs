using System.Diagnostics;
using BedAutomation.Data;
using BedAutomation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BedAutomation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Patient"))
            {
                // Redirect patients to their reservations page
                return RedirectToAction("MyReservations", "Patient");
            }
            else if (roles.Contains("Doctor"))
            {
                // Redirect doctors to their profile/dashboard
                return RedirectToAction("MyProfile", "Doctor");
            }

            // Sync bed occupancy status with active reservations
            await SyncBedOccupancyStatus();

            // Admin dashboard with comprehensive analytics
            var totalPatients = await _context.Patients.CountAsync();
            var totalDoctors = await _context.Doctors.CountAsync();
            var totalRooms = await _context.Rooms.CountAsync();
            var totalBeds = await _context.Beds.CountAsync(b => b.IsActive);
            
            // Get occupied bed IDs from active reservations
            var occupiedBedIds = await _context.Reservations
                .Where(r => r.Status == "Active" || r.Status == "Reserved")
                .Select(r => r.BedId)
                .Distinct()
                .ToListAsync();
            
            var occupiedBeds = occupiedBedIds.Count;
            var availableBeds = totalBeds - occupiedBeds;
            var activeReservations = await _context.Reservations.CountAsync(r => r.Status == "Active" || r.Status == "Reserved");
            var todayReservations = await _context.Reservations.CountAsync(r => r.CheckInDate.Date == DateTime.UtcNow.Date);
            
            // Occupancy rate based on active reservations
            var occupancyRate = totalBeds > 0 ? Math.Round((double)occupiedBeds / totalBeds * 100, 1) : 0;
            


            // Recent reservations
            var recentReservations = await _context.Reservations
                .Include(r => r.Patient)
                .Include(r => r.Bed)
                    .ThenInclude(b => b.Room)
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();



            // Status distribution
            var statusStats = await _context.Reservations
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Monthly reservations trend (last 6 months)
            var monthlyTrend = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.UtcNow.AddMonths(-i);
                var count = await _context.Reservations
                    .CountAsync(r => r.CheckInDate.Month == targetDate.Month && r.CheckInDate.Year == targetDate.Year);
                monthlyTrend.Add(new { Month = targetDate.ToString("MMM yyyy"), Count = count });
            }

            var stats = new
            {
                TotalPatients = totalPatients,
                TotalDoctors = totalDoctors,
                TotalRooms = totalRooms,
                TotalBeds = totalBeds,
                AvailableBeds = availableBeds,
                OccupiedBeds = occupiedBeds,
                ActiveReservations = activeReservations,
                TodayReservations = todayReservations,
                OccupancyRate = occupancyRate,
                RecentReservations = recentReservations,
                StatusStats = statusStats,
                MonthlyTrend = monthlyTrend
            };

            ViewBag.Stats = stats;
            return View();
        }

        private async Task SyncBedOccupancyStatus()
        {
            try
            {
                // Get all beds
                var allBeds = await _context.Beds.ToListAsync();
                
                // Get occupied bed IDs from active reservations
                var occupiedBedIds = await _context.Reservations
                    .Where(r => r.Status == "Active" || r.Status == "Reserved")
                    .Select(r => r.BedId)
                    .Distinct()
                    .ToListAsync();
                
                // Update bed occupancy status
                foreach (var bed in allBeds)
                {
                    var shouldBeOccupied = occupiedBedIds.Contains(bed.Id);
                    if (bed.IsOccupied != shouldBeOccupied)
                    {
                        bed.IsOccupied = shouldBeOccupied;
                        _context.Update(bed);
                    }
                }
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing bed occupancy status");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
