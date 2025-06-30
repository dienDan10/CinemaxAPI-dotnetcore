using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;

namespace CinemaxAPI.Repositories.Impl
{
    public class BookingDetailRepository : Repository<BookingDetail>, IBookingDetailRepository
    {
        public BookingDetailRepository(CinemaxServerDbContext context) : base(context)
        {

        }
    }
}
