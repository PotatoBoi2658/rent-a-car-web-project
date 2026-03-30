using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Models
{
    /// <summary>
    /// Represents a user in the Rent-a-Car system.
    /// Extends the default ASP.NET Identity User with custom properties.
    /// </summary>
    public class User : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "EGN must be 10 digits.")]
        [Display(Name = "EGN")]
        public string EGN { get; set; }

        [Display(Name = "Is Administrator")]
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// Navigation property for user's reservations.
        /// </summary>
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
