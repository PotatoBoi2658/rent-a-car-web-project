using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rent_a_car.Models
{
    /// <summary>
    /// Represents a car reservation request made by a user.
    /// </summary>
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Car")]
        public int CarId { get; set; }

        [Required]
        [Display(Name = "User")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed

        [Display(Name = "Total Price")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Navigation property for the reserved car.
        /// </summary>
        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; }

        /// <summary>
        /// Navigation property for the user who made the reservation.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        /// <summary>
        /// Calculates the total rental price based on the car's daily rate and rental duration.
        /// </summary>
        public void CalculateTotalPrice()
        {
            if (Car != null && StartDate < EndDate)
            {
                int days = (int)(EndDate - StartDate).TotalDays;
                TotalPrice = Car.PricePerDay * days;
            }
        }
    }
}
