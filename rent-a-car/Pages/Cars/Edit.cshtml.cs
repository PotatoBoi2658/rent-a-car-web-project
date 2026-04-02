using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rent_a_car.Data;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Pages.Cars
{
    [Authorize(Roles = "Administrator")]
    public class EditCarModel : PageModel
    {
        private readonly RentACarDbContext _context;
        private readonly ILogger<EditCarModel> _logger;

        public EditCarModel(RentACarDbContext context, ILogger<EditCarModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            [Display(Name = "Brand")]
            public string Brand { get; set; }

            [Required]
            [StringLength(100)]
            [Display(Name = "Model")]
            public string Model { get; set; }

            [Required]
            [Range(1900, 2100)]
            [Display(Name = "Year")]
            public int Year { get; set; }

            [Required]
            [Range(1, 20)]
            [Display(Name = "Passenger Seats")]
            public int PassengerSeats { get; set; }

            [Required]
            [Range(0.01, 10000)]
            [Display(Name = "Price Per Day")]
            public decimal PricePerDay { get; set; }

            [StringLength(500)]
            [Display(Name = "Description")]
            public string Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                PassengerSeats = car.PassengerSeats,
                PricePerDay = car.PricePerDay,
                Description = car.Description
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var car = await _context.Cars.FindAsync(Input.Id);
                if (car == null)
                {
                    return NotFound();
                }

                car.Brand = Input.Brand;
                car.Model = Input.Model;
                car.Year = Input.Year;
                car.PassengerSeats = Input.PassengerSeats;
                car.PricePerDay = Input.PricePerDay;
                car.Description = Input.Description ?? string.Empty;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Car {CarId} updated: {Brand} {Model}", car.Id, car.Brand, car.Model);

                return RedirectToPage("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating car {CarId}.", Input.Id);
                ModelState.AddModelError(string.Empty, "The record was modified by another user.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating car {CarId}.", Input.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the car.");
                return Page();
            }
        }
    }
}