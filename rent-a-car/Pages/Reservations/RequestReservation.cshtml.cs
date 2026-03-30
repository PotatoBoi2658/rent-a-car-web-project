using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Data;
using rent_a_car.Models;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Pages.Reservations
{
    [Authorize]
    public class RequestReservationModel : PageModel
    {
        private readonly RentACarDbContext _context;
        private readonly UserManager<User> _userManager;

        public RequestReservationModel(RentACarDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public int CarId { get; set; }
        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [BindProperty]
        public string? Message { get; set; }

        public Car? Car { get; set; }

        public async Task<IActionResult> OnGetAsync(int carId, DateTime startDate, DateTime endDate)
        {
            Car = await _context.Cars.FindAsync(carId);
            if (Car == null)
                return NotFound();

            CarId = carId;
            StartDate = startDate;
            EndDate = endDate;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Car = await _context.Cars.FindAsync(CarId);
            if (Car == null)
                return NotFound();

            if (StartDate >= EndDate)
            {
                ModelState.AddModelError(string.Empty, "End date must be after start date.");
                return Page();
            }

            var userId = _userManager.GetUserId(User);
            var reservation = new Reservation
            {
                CarId = CarId,
                UserId = userId,
                StartDate = StartDate,
                EndDate = EndDate,
                Status = "Pending",
                TotalPrice = Car.PricePerDay * (EndDate - StartDate).Days
            };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation request submitted!";
            return RedirectToPage("/Reservations/Index");
        }
    }
}