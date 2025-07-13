using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(CinemaxServerDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Seat>> GetBookedSeatsByShowtimeId(int showtimeId)
        {
            var seats = await _context.BookingDetails
                .AsNoTracking()
                .Where(b => b.Booking.ShowTimeId == showtimeId && (b.Booking.IsActive || b.Booking.CreatedAt.AddMinutes(5) >= DateTime.Now))
                .Select(b => b.Seat)
                .ToListAsync();

            return seats;
        }
    }
}
