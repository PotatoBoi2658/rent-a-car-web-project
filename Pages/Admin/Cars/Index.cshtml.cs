using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace rent_a_car.Pages.Admin.Cars
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        // Admin-only page example
    }
}