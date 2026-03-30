using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using rent_a_car.Models;

namespace rent_a_car.Data
{
    public class RentACarDbContext : IdentityDbContext<User>
    {
        public RentACarDbContext(DbContextOptions<RentACarDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique constraints required by заданията: уникално EGN, UserName и Email
            builder.Entity<User>(b =>
            {
                b.HasIndex(u => u.EGN).IsUnique();
                b.HasIndex(u => u.UserName).IsUnique();
                b.HasIndex(u => u.Email).IsUnique();
            });

            // Example: precision for price
            builder.Entity<Car>().Property(c => c.PricePerDay).HasPrecision(10, 2);
        }
    }
}
