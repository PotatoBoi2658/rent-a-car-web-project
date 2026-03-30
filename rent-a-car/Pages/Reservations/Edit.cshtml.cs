using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Data;
using rent_a_car.Models;
using rent_a_car.Services;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Pages.Reservations
{
    [Authorize]
    public class EditReservationModel : PageModel
    {
        private readonly RentACarDbContext _context;
        private readonly IReservationService _reservationService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<EditReservationModel> _logger;

        public EditReservationModel(
            RentACarDbContext context,
            IReservationService reservationService,
            UserManager<User> userManager,
            ILogger<EditReservationModel> logger)
        {
            _context = context;
            _reservationService = reservationService;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public Reservation Reservation { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Start Date")]
            public DateTime StartDate { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "End Date")]
            public DateTime EndDate { get; set; }
        }

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

            // Only pending reservations can be edited
            if (Reservation.Status != "Pending")
            {
                return RedirectToPage("Index");
            }

            Input = new InputModel
            {
                Id = Reservation.Id,
                StartDate = Reservation.StartDate,
                EndDate = Reservation.EndDate
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var reservation = await _reservationService.GetReservationByIdAsync(Input.Id);
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

            if (Input.StartDate >= Input.EndDate)
            {
                ModelState.AddModelError(nameof(Input.EndDate), "End date must be after start date.");
                return Page();
            }

            try
            {
                // Check if car is available for the new dates
                bool isAvailable = await _reservationService.IsCarAvailableAsync(
                    reservation.CarId,
                    Input.StartDate,
                    Input.EndDate);

                if (!isAvailable)
                {
                    ModelState.AddModelError(string.Empty, "The car is not available for the selected dates.");
                    return Page();
                }

                reservation.StartDate = Input.StartDate;
                reservation.EndDate = Input.EndDate;
                reservation.CalculateTotalPrice();

                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reservation {ReservationId} updated.", Input.Id);
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation {ReservationId}.", Input.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the reservation.");
                return Page();
            }
        }
    }
}