using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Data;
using rent_a_car.Models;

namespace rent_a_car.Pages.Cars
{
    [Authorize(Roles = "Administrator")]
    public class DeleteCarModel : PageModel
    {
        private readonly RentACarDbContext _context;
        private readonly ILogger<DeleteCarModel> _logger;

        public DeleteCarModel(RentACarDbContext context, ILogger<DeleteCarModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Car Car { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Car = await _context.Cars.FindAsync(id);
            if (Car == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            try
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Car {CarId} deleted: {Brand} {Model}", car.Id, car.Brand, car.Model);

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting car {CarId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the car.");
                return Page();
            }
        }
    }
}