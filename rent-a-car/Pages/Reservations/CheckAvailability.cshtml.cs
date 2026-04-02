using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Models;
using rent_a_car.Services;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Pages.Reservations
{
    [Authorize]
    public class CheckAvailabilityModel : PageModel
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<CheckAvailabilityModel> _logger;

        public CheckAvailabilityModel(
            IReservationService reservationService,
            ILogger<CheckAvailabilityModel> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<Car> AvailableCars { get; set; } = new();

        public bool CheckedAvailability { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; }
        }

        public void OnGet()
        {
            Input.StartDate = DateTime.Today;
            Input.EndDate = DateTime.Today.AddDays(1);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var today = DateTime.Today;

            if (Input.StartDate < today)
            {
                ModelState.AddModelError(nameof(Input.StartDate), "Start date cannot be before today.");
                return Page();
            }

            if (Input.EndDate <= today)
            {
                ModelState.AddModelError(nameof(Input.EndDate), "End date must be after today.");
                return Page();
            }

            if (Input.StartDate >= Input.EndDate)
            {
                ModelState.AddModelError(nameof(Input.EndDate), "End date must be after start date.");
                return Page();
            }

            try
            {
                AvailableCars = await _reservationService.GetAvailableCarsAsync(Input.StartDate, Input.EndDate);
                CheckedAvailability = true;
                return Page();
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An error occurred while checking availability.");
                return Page();
            }
        }
    }
}