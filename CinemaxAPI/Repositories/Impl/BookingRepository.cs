using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(CinemaxServerDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<int>> GetAllBookingSeatIds(int showtimeId)
        {
            // only get the booked seats that isActive (has been paid) or created 5 minutes recently
            var bookedSeatIds = await _context.BookingDetails
                .AsNoTracking()
                .Where(b => b.Booking.ShowTimeId == showtimeId)
                .Select(b => b.SeatId)
                .Where(seatId => seatId.HasValue)
                .Select(seatId => seatId.Value)
                .ToListAsync();
            return bookedSeatIds;
        }
    }
}
