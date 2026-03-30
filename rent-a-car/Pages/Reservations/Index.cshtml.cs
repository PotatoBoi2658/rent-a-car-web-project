using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rent_a_car.Data;
using rent_a_car.Models;

namespace rent_a_car.Pages.Reservations
{
    [Authorize]
    public class ReservationsIndexModel : PageModel
    {
        private readonly RentACarDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ReservationsIndexModel> _logger;

        public ReservationsIndexModel(
            RentACarDbContext context,
            UserManager<User> userManager,
            ILogger<ReservationsIndexModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public List<Reservation> Reservations { get; set; } = new();
        public List<Reservation> PendingReservations { get; set; } = new();
        public int PendingCount { get; set; }
        public bool ShowPendingOnly { get; set; }

        public async Task OnGetAsync(bool pending = false)
        {
            ShowPendingOnly = pending;

            if (User.IsInRole("Administrator"))
            {
                // Admin sees all reservations
                Reservations = await _context.Reservations
                    .Include(r => r.Car)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.StartDate)
                    .ToListAsync();

                // Get pending requests separately for the tab
                PendingReservations = Reservations
                    .Where(r => r.Status == "Pending")
                    .ToList();

                PendingCount = PendingReservations.Count;
            }
            else
            {
                // Regular users see only their own reservations
                var userId = _userManager.GetUserId(User);
                Reservations = await _context.Reservations
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Car)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.StartDate)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            try
            {
                reservation.Status = "Approved";
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reservation {ReservationId} approved by admin.", id);
                TempData["SuccessMessage"] = "Reservation approved successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving reservation {ReservationId}.", id);
                TempData["ErrorMessage"] = "An error occurred while approving the reservation.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            try
            {
                reservation.Status = "Rejected";
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reservation {ReservationId} rejected by admin.", id);
                TempData["SuccessMessage"] = "Reservation rejected.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting reservation {ReservationId}.", id);
                TempData["ErrorMessage"] = "An error occurred while rejecting the reservation.";
                return RedirectToPage();
            }
        }
    }
}