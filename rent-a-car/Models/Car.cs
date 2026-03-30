using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rent_a_car.Models
{
    /// <summary>
    /// Represents a car available for rental in the system.
    /// </summary>
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Brand")]
        [StringLength(100)]
        public string Brand { get; set; }

        [Required]
        [Display(Name = "Model")]
        [StringLength(100)]
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
        [Display(Name = "Price Per Day")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 10000)]
        public decimal PricePerDay { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Navigation property for reservations associated with this car.
        /// </summary>
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        /// <summary>
        /// Gets the full name of the car (Brand + Model).
        /// </summary>
        [NotMapped]
        public string FullName => $"{Brand} {Model}";
    }
}
