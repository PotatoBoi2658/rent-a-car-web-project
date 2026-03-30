using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rent_a_car.Data;
using rent_a_car.Models;

namespace rent_a_car.Pages.Cars
{
    public class CarsIndexModel : PageModel
    {
        private readonly RentACarDbContext _context;

        public CarsIndexModel(RentACarDbContext context)
        {
            _context = context;
        }

        public List<Car> Cars { get; set; } = new();

        public async Task OnGetAsync()
        {
            Cars = await _context.Cars.ToListAsync();
        }
    }
}