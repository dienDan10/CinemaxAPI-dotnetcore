using CinemaxAPI.Data;

namespace CinemaxAPI.Repositories.Impl
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CinemaxServerDbContext _db;
        public IProvinceRepository Province { get; private set; }
        public ITheaterRepository Theater { get; private set; }
        public IMovieRepository Movie { get; private set; }
        public IShowTimeRepository ShowTime { get; private set; }
        public ISeatRepository Seat { get; private set; }
        public IScreenRepository Screen { get; private set; }
        public IBookingRepository Booking { get; private set; }
        public IBookingDetailRepository BookingDetail { get; private set; }
        public IConcessionRepository Concession { get; private set; }
        public IConcessionOrderRepository ConcessionOrder { get; private set; }
        public IConcessionOrderDetailRepository ConcessionOrderDetail { get; private set; }
        public IPaymentRepository Payment { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }


        public UnitOfWork(CinemaxServerDbContext db)
        {
            _db = db;
            Province = new ProvinceRepository(_db);
            Theater = new TheaterRepository(_db);
            Movie = new MovieRepository(_db);
            ShowTime = new ShowTimeRepository(_db);
            Seat = new SeatRepository(_db);
            Screen = new ScreenRepository(_db);
            Booking = new BookingRepository(_db);
            BookingDetail = new BookingDetailRepository(_db);
            Concession = new ConcessionRepository(_db);
            ConcessionOrder = new ConcessionOrderRepository(_db);
            ConcessionOrderDetail = new ConcessionOrderDetailRepository(_db);
            Payment = new PaymentRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
