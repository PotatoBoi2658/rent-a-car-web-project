using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Models;
using rent_a_car.Services;

namespace rent_a_car.Pages.Reservations
{
    [Authorize]
    public class DeleteReservationModel : PageModel
    {
        private readonly IReservationService _reservationService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<DeleteReservationModel> _logger;

        public DeleteReservationModel(
            IReservationService reservationService,
            UserManager<User> userManager,
            ILogger<DeleteReservationModel> logger)
        {
            _reservationService = reservationService;
            _userManager = userManager;
            _logger = logger;
        }

        public Reservation Reservation { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reservation = await _reservationService.GetReservationByIdAsync(id);
            if (Reservation == null)
            {
                return NotFound();
            }

            // Check authorization
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Administrator") && Reservation.UserId != userId)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Check authorization
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Administrator") && reservation.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                var result = await _reservationService.DeleteReservationAsync(id);
                if (result)
                {
                    _logger.LogInformation("Reservation {ReservationId} deleted.", id);
                    return RedirectToPage("Index");
                }

                ModelState.AddModelError(string.Empty, "Failed to delete reservation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reservation {ReservationId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the reservation.");
            }

            return Page();
        }
    }
}