using rent_a_car.Models;

namespace rent_a_car.Services
{
    /// <summary>
    /// Defines the contract for reservation-related business logic.
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Gets all available cars for a given date range.
        /// </summary>
        /// <param name="startDate">The start date of the rental period.</param>
        /// <param name="endDate">The end date of the rental period.</param>
        /// <returns>A list of cars that are available during the specified period.</returns>
        Task<List<Car>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Checks if a specific car is available during a given period.
        /// </summary>
        /// <param name="carId">The ID of the car.</param>
        /// <param name="startDate">The start date of the rental period.</param>
        /// <param name="endDate">The end date of the rental period.</param>
        /// <returns>True if the car is available; otherwise, false.</returns>
        Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Creates a new reservation for a user.
        /// </summary>
        /// <param name="reservation">The reservation to create.</param>
        /// <returns>True if the reservation was created successfully; otherwise, false.</returns>
        Task<bool> CreateReservationAsync(Reservation reservation);

        /// <summary>
        /// Gets all reservations (for administrators).
        /// </summary>
        /// <returns>A list of all reservations.</returns>
        Task<List<Reservation>> GetAllReservationsAsync();

        /// <summary>
        /// Gets all reservations for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A list of the user's reservations.</returns>
        Task<List<Reservation>> GetUserReservationsAsync(string userId);

        /// <summary>
        /// Approves a pending reservation.
        /// </summary>
        /// <param name="reservationId">The ID of the reservation to approve.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        Task<bool> ApproveReservationAsync(int reservationId);

        /// <summary>
        /// Rejects a pending reservation.
        /// </summary>
        /// <param name="reservationId">The ID of the reservation to reject.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        Task<bool> RejectReservationAsync(int reservationId);

        /// <summary>
        /// Deletes a reservation.
        /// </summary>
        /// <param name="reservationId">The ID of the reservation to delete.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        Task<bool> DeleteReservationAsync(int reservationId);

        /// <summary>
        /// Gets a specific reservation by ID.
        /// </summary>
        /// <param name="reservationId">The ID of the reservation.</param>
        /// <returns>The reservation, or null if not found.</returns>
        Task<Reservation> GetReservationByIdAsync(int reservationId);
    }
}