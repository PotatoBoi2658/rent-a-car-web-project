using Microsoft.EntityFrameworkCore;
using rent_a_car.Data;
using rent_a_car.Models;

namespace rent_a_car.Services
{
    /// <summary>
    /// Provides business logic for managing reservations and car availability.
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly RentACarDbContext _context;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(RentACarDbContext context, ILogger<ReservationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all cars that are available (not reserved) during the specified period.
        /// </summary>
        public async Task<List<Car>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Validate dates
                if (startDate >= endDate)
                {
                    _logger.LogWarning("Invalid date range: start date must be before end date.");
                    return new List<Car>();
                }

                // Get all cars that don't have conflicting reservations
                var availableCars = await _context.Cars
                    .Where(c => !c.Reservations.Any(r =>
                        r.Status == "Approved" &&
                        r.StartDate < endDate &&
                        r.EndDate > startDate))
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} available cars for period {StartDate} to {EndDate}",
                    availableCars.Count, startDate, endDate);

                return availableCars;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available cars.");
                throw;
            }
        }

        /// <summary>
        /// Checks if a specific car is available during the given period.
        /// </summary>
        public async Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                {
                    _logger.LogWarning("Invalid date range for car {CarId}", carId);
                    return false;
                }

                var conflict = await _context.Reservations
                    .Where(r =>
                        r.CarId == carId &&
                        r.Status == "Approved" &&
                        r.StartDate < endDate &&
                        r.EndDate > startDate)
                    .AnyAsync();

                return !conflict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for car {CarId}", carId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new reservation after validating car availability.
        /// </summary>
        public async Task<bool> CreateReservationAsync(Reservation reservation)
        {
            try
            {
                if (reservation == null)
                {
                    _logger.LogWarning("Null reservation provided.");
                    return false;
                }

                if (reservation.StartDate >= reservation.EndDate)
                {
                    _logger.LogWarning("Invalid reservation dates: start must be before end.");
                    return false;
                }

                // Verify the car exists
                var car = await _context.Cars.FindAsync(reservation.CarId);
                if (car == null)
                {
                    _logger.LogWarning("Car {CarId} not found.", reservation.CarId);
                    return false;
                }

                // Check car availability
                bool isAvailable = await IsCarAvailableAsync(reservation.CarId, reservation.StartDate, reservation.EndDate);
                if (!isAvailable)
                {
                    _logger.LogWarning("Car {CarId} is not available for the requested period.", reservation.CarId);
                    return false;
                }

                // Calculate total price
                reservation.CalculateTotalPrice();

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reservation {ReservationId} created successfully for user {UserId}.",
                    reservation.Id, reservation.UserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all reservations (admin only).
        /// </summary>
        public async Task<List<Reservation>> GetAllReservationsAsync()
        {
            try
            {
                return await _context.Reservations
                    .Include(r => r.Car)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reservations.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all reservations for a specific user.
        /// </summary>
        public async Task<List<Reservation>> GetUserReservationsAsync(string userId)
        {
            try
            {
                return await _context.Reservations
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Car)
                    .OrderByDescending(r => r.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations for user {UserId}.", userId);
                throw;
            }
        }

        /// <summary>
        /// Approves a pending reservation.
        /// </summary>
        public async Task<bool> ApproveReservationAsync(int reservationId)
        {
            try
            {
                var reservation = await _context.Reservations.FindAsync(reservationId);
                if (reservation == null)
                {
                    _logger.LogWarning("Reservation {ReservationId} not found.", reservationId);
                    return false;
                }

                if (reservation.Status != "Pending")
                {
                    _logger.LogWarning("Cannot approve reservation {ReservationId} with status {Status}.",
                        reservationId, reservation.Status);
                    return false;
                }

                reservation.Status = "Approved";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reservation {ReservationId} approved.", reservationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving reservation {ReservationId}.", reservationId);
                throw;
            }
        }

        /// <summary>
        /// Rejects a pending reservation.
        /// </summary>
        public async Task<bool> RejectReservationAsync(int reservationId)
        {
            try
            {
                var reservation = await _context.Reservations.FindAsync(reservationId);
                if (reservation == null)
                {
                    _logger.LogWarning("Reservation {ReservationId} not found.", reservationId);
                    return false;
                }

                if (reservation.Status != "Pending")
                {
                    _logger.LogWarning("Cannot reject reservation {ReservationId} with status {Status}.",
                        reservationId, reservation.Status);
                    return false;
                }

                reservation.Status = "Rejected";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reservation {ReservationId} rejected.", reservationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting reservation {ReservationId}.", reservationId);
                throw;
            }
        }

        /// <summary>
        /// Deletes a reservation (typically pending reservations).
        /// </summary>
        public async Task<bool> DeleteReservationAsync(int reservationId)
        {
            try
            {
                var reservation = await _context.Reservations.FindAsync(reservationId);
                if (reservation == null)
                {
                    _logger.LogWarning("Reservation {ReservationId} not found.", reservationId);
                    return false;
                }

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reservation {ReservationId} deleted.", reservationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reservation {ReservationId}.", reservationId);
                throw;
            }
        }

        /// <summary>
        /// Gets a specific reservation by ID.
        /// </summary>
        public async Task<Reservation> GetReservationByIdAsync(int reservationId)
        {
            try
            {
                return await _context.Reservations
                    .Include(r => r.Car)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation {ReservationId}.", reservationId);
                throw;
            }
        }
    }
}