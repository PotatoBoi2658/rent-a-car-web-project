using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Models;
using rent_a_car.Services;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Pages.Reservations
{
    [Authorize]
    public class CreateReservationModel : PageModel
    {
        private readonly IReservationService _reservationService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CreateReservationModel> _logger;

        public CreateReservationModel(
            IReservationService reservationService,
            UserManager<User> userManager,
            ILogger<CreateReservationModel> logger)
        {
            _reservationService = reservationService;
            _userManager = userManager;
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
            [Display(Name = "Start Date")]
            public DateTime StartDate { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "End Date")]
            public DateTime EndDate { get; set; }

            [Display(Name = "Car")]
            public int CarId { get; set; }
        }

        public async Task OnGetAsync(int? carId = null)
        {
            // If carId is provided in query string, pre-select it
            if (carId.HasValue)
            {
                Input.CarId = carId.Value;
            }

            Input.StartDate = DateTime.Today;
            Input.EndDate = DateTime.Today.AddDays(1);
        }

        public async Task<IActionResult> OnPostCheckAvailabilityAsync()
        {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking car availability.");
                ModelState.AddModelError(string.Empty, "An error occurred while checking availability.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input.CarId <= 0)
            {
                ModelState.AddModelError(nameof(Input.CarId), "Please select a car.");
                return Page();
            }

            if (Input.StartDate >= Input.EndDate)
            {
                ModelState.AddModelError(nameof(Input.EndDate), "End date must be after start date.");
                return Page();
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var reservation = new Reservation
                {
                    CarId = Input.CarId,
                    UserId = userId,
                    StartDate = Input.StartDate,
                    EndDate = Input.EndDate,
                    Status = "Pending"
                };

                var result = await _reservationService.CreateReservationAsync(reservation);
                if (result)
                {
                    _logger.LogInformation("Reservation created successfully for user {UserId}.", userId);
                    return RedirectToPage("Index");
                }

                ModelState.AddModelError(string.Empty, "Failed to create reservation. The car may no longer be available.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the reservation.");
                return Page();
            }
        }
    }
}