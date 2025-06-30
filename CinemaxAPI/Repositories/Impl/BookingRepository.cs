using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(CinemaxServerDbContext context) : base(context)
        {

        }

        public IEnumerable<int> GetAllBookingSeatIds(int showtimeId)
        {
            // only get the booked seats that isActive (has been paid) or created 5 minutes recently
            var bookedSeatIds = _context.BookingDetails
                .AsNoTracking()
                .Where(b => b.Booking.ShowTimeId == showtimeId && (b.Booking.IsActive || b.Booking.CreatedAt.AddMinutes(5) >= DateTime.Now))
                .Select(b => b.SeatId)
                .Where(seatId => seatId.HasValue)
                .Select(seatId => seatId.Value)
                .ToList();
            return bookedSeatIds;
        }
    }
}
