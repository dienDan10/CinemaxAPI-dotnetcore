using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(CinemaxServerDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<RevenueItemDTO>> GetRevenueItemsAsync(DateTime startDate, DateTime endDate, int? theaterId = null)
        {
            //var query = _context.Payments
            //    .Where(p => p.PaymentDate >= startDate
            //        && p.PaymentDate <= endDate
            //        && p.PaymentStatus == Constants.PaymentStatus_Success
            //        && (!theaterId.HasValue || p.Booking.ShowTime.Screen.Theater.Id == theaterId.Value))
            //    .Select(p => new RevenueItemDTO
            //    {
            //        Date = DateOnly.FromDateTime(p.PaymentDate),
            //        Payment = new PaymentConciseDTO
            //        {
            //            Id = p.Id,
            //            PaymentDate = p.PaymentDate,
            //            TicketCount = p.Booking.BookingDetails.Count,
            //            Amount = p.Amount,
            //            PaymentStatus = p.PaymentStatus
            //        },
            //        Movie = new MovieConciseDTO
            //        {
            //            Id = p.Booking.ShowTime.Movie.Id,
            //            Title = p.Booking.ShowTime.Movie.Title
            //        }
            //    });

            var query = from p in _context.Payments
                        where p.PaymentDate >= startDate &&
                              p.PaymentDate <= endDate &&
                              p.PaymentStatus == Constants.PaymentStatus_Success
                        let booking = p.Booking
                        let showTime = booking != null ? booking.ShowTime : null
                        let screen = showTime != null ? showTime.Screen : null
                        let theater = screen != null ? screen.Theater : null
                        let ticketCount = booking != null ? booking.BookingDetails.Count : 0
                        let bookingDetails = booking != null ? booking.BookingDetails : null
                        let ticketPrices = bookingDetails != null ? bookingDetails.Sum(bd => bd.TicketPrice) : 0
                        let concessions = p.ConcessionOrder != null ? p.ConcessionOrder.ConcessionOrderDetails.Select(cod => new ConcessionConciseDTO
                        {

                            Id = cod.Concession.Id,
                            Name = cod.Concession.Name,
                            Quantity = cod.Quantity,
                            Amount = cod.Concession.Price * cod.Quantity
                        }).ToList() : new List<ConcessionConciseDTO>()
                        orderby p.PaymentDate ascending
                        select new RevenueItemDTO
                        {
                            Date = DateOnly.FromDateTime(p.PaymentDate),
                            Payment = new PaymentConciseDTO
                            {
                                Id = p.Id,
                                PaymentDate = p.PaymentDate,
                                Amount = p.TotalAmount,
                                PaymentStatus = p.PaymentStatus
                            },
                            Theater = new TheaterConciseDTO
                            {
                                Id = theater != null ? theater.Id : 0,
                                TheaterName = theater != null ? theater.Name : "Unknown",
                                Amount = p.TotalAmount
                            },
                            ShowTime = new ShowtimeConciseDTO
                            {
                                Id = showTime != null ? showTime.Id : 0,
                                StartTime = showTime != null ? showTime.StartTime : TimeSpan.Zero,
                                EndTime = showTime != null ? showTime.EndTime : TimeSpan.Zero,
                                TicketCount = ticketCount,
                                Amount = ticketPrices
                            },
                            Movie = new MovieConciseDTO
                            {
                                Id = showTime != null ? showTime.Movie.Id : 0,
                                Title = showTime != null ? showTime.Movie.Title : "Unknown",
                                TicketCount = ticketCount,
                                Amount = ticketPrices
                            },
                            Concessions = concessions
                        };


            return await query.ToListAsync();
        }

        //public PaymentListVM GetAllInTheater(DateTime startDate, DateTime endDate, int theaterId, int start, int length)
        //{
        //    IQueryable<Payment> query = _context.Payments;
        //    query = query.Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate && p.PaymentStatus == Constant.PaymentStatus_Success);

        //    if (theaterId != 0)
        //    {
        //        query = query.Where(p => p.Booking.ShowTime.Screen.TheaterId == theaterId);
        //    }

        //    int count = query.Count();

        //    var paymentList = query.OrderByDescending(p => p.PaymentDate)
        //        .Skip(start).Take(length)
        //        .Select(p => new PaymentListItem
        //        {
        //            PaymentId = p.Id,
        //            UserName = p.Name,
        //            Amount = p.Amount,
        //            PaymentMethod = p.PaymentMethod,
        //            PaymentDate = p.PaymentDate,
        //            PaymentStatus = p.PaymentStatus
        //        }).ToList();

        //    return new PaymentListVM
        //    {
        //        RecordsTotal = count,
        //        ListItem = paymentList
        //    };
        //}

        public void UpdateStatus(int id, string status)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (!string.IsNullOrEmpty(status))
            {
                payment.PaymentStatus = status;
            }
            payment.LastUpdatedAt = DateTime.Now;
        }

        //public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        //{
        //    var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
        //    if (!string.IsNullOrEmpty(sessionId))
        //    {
        //        payment.SessionId = sessionId;
        //    }
        //    if (!string.IsNullOrEmpty(paymentIntentId))
        //    {
        //        payment.PaymentIntentId = paymentIntentId;
        //    }
        //    payment.LastUpdatedAt = DateTime.Now;
        //}
    }
}
