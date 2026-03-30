using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Data;
using rent_a_car.Models;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Pages.Cars
{
    [Authorize(Roles = "Administrator")]
    public class CreateCarModel : PageModel
    {
        private readonly RentACarDbContext _context;
        private readonly ILogger<CreateCarModel> _logger;

        public CreateCarModel(RentACarDbContext context, ILogger<CreateCarModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
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

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var car = new Car
                {
                    Brand = Input.Brand,
                    Model = Input.Model,
                    Year = Input.Year,
                    PassengerSeats = Input.PassengerSeats,
                    PricePerDay = Input.PricePerDay,
                    Description = Input.Description ?? string.Empty
                };

                _context.Cars.Add(car);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Car {CarId} created: {Brand} {Model}", car.Id, car.Brand, car.Model);
                TempData["SuccessMessage"] = $"✓ {car.FullName} has been added to the fleet!";

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating car.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the car.");
                return Page();
            }
        }
    }
}