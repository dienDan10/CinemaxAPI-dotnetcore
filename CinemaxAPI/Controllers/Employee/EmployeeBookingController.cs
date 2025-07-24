using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Services;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Employee
{
    [Route("api/employee/bookings")]
    [ApiController]
    public class EmployeeBookingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IQRCodeService _qrCodeService;
        public EmployeeBookingController(IUnitOfWork unitOfWork, IMapper mapper, IQRCodeService qrCodeService, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
        }

        [HttpGet("showtimes")]
        [Authorize(Roles = Constants.Role_Employee)]
        public async Task<IActionResult> GetShowTimes([FromQuery] EmployeeGetShowtimesRequestDTO request)
        {
            var showTimes = await _unitOfWork.ShowTime.GetAllAsync(
                st => st.Screen.TheaterId == request.TheaterId && st.Date == DateTime.Parse(request.Date),
                includeProperties: "Movie,Screen");

            var showtimeGroups = showTimes.GroupBy(st => new { st.Date, st.Movie })
                .Select(g => new
                {
                    g.Key.Date,
                    Movie = _mapper.Map<MovieDTO>(g.Key.Movie),
                    ShowTimes = g.Select(st => _mapper.Map<ShowTimeDTO>(st)).ToList()
                });

            return Ok(new SuccessResponseDTO
            {
                Data = showtimeGroups,
                Message = "ShowTimes retrieved successfully."
            });
        }

        [HttpPost]
        [Authorize(Roles = Constants.Role_Employee)]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDTO bookingRequest)
        {
            // check if seats have been booked
            var bookedSeats = await _unitOfWork.Seat.GetBookedSeatsByShowtimeId(bookingRequest.ShowtimeId);
            foreach (var seat in bookingRequest.Seats)
            {
                if (bookedSeats.Any(bs => bs.Id == seat.Id))
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = $"Seat {seat.Name} has already been booked.",
                        StatusCode = 400
                    });
                }
            }

            // calculate total amount of booking
            var showtime = await _unitOfWork.ShowTime.GetOneAsync(st => st.Id == bookingRequest.ShowtimeId);
            if (showtime == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Showtime not found.",
                    StatusCode = 404
                });
            }

            decimal totalAmount = 0;

            foreach (var seat in bookingRequest.Seats)
            {
                if (seat.SeatType == Constants.SeatType_Normal)
                    totalAmount += showtime.TicketPrice;
                else if (seat.SeatType == Constants.SeatType_Vip)
                    totalAmount += showtime.VipTicketPrice;
            }

            // create booking
            var booking = new Booking
            {
                ShowTimeId = bookingRequest.ShowtimeId,
                BookingDate = DateTime.Now,
                TotalAmount = totalAmount,
                BookingStatus = Constants.BookingStatus_Success,
                IsActive = true
            };

            // save booking
            await _unitOfWork.Booking.AddAsync(booking);
            await _unitOfWork.SaveAsync();

            // create booking details for each seat
            foreach (var seat in bookingRequest.Seats)
            {
                var bookingDetail = new BookingDetail
                {
                    BookingId = booking.Id,
                    SeatId = seat.Id,
                    SeatName = seat.Name,
                    TicketPrice = seat.SeatType == Constants.SeatType_Vip ? showtime.VipTicketPrice : showtime.TicketPrice
                };

                // save booking detail
                await _unitOfWork.BookingDetail.AddAsync(bookingDetail);
                await _unitOfWork.SaveAsync();
            }

            // create concession order
            ConcessionOrder concessionOrder = null;

            if (bookingRequest.Concessions.Count > 0)
            {
                // calculate total price of concessions
                decimal totalConcessionPrice = 0;
                foreach (var concession in bookingRequest.Concessions)
                {
                    var concessionItem = await _unitOfWork.Concession.GetOneAsync(c => c.Id == concession.Id && c.IsActive && !c.IsRemoved);
                    if (concessionItem == null)
                    {
                        return NotFound(new ErrorResponseDTO
                        {
                            Message = $"Concession selected not found.",
                            StatusCode = 404
                        });
                    }

                    totalConcessionPrice += concessionItem.Price * concession.Quantity;
                }
                concessionOrder = new ConcessionOrder
                {
                    OrderDate = DateTime.Now,
                    TotalPrice = totalConcessionPrice,
                    IsActive = true
                };

                // save concession order
                await _unitOfWork.ConcessionOrder.AddAsync(concessionOrder);
                await _unitOfWork.SaveAsync();

                // create concession order details
                foreach (var concession in bookingRequest.Concessions)
                {
                    var concessionItem = await _unitOfWork.Concession.GetOneAsync(c => c.Id == concession.Id && c.IsActive && !c.IsRemoved);
                    if (concessionItem == null)
                    {
                        return NotFound(new ErrorResponseDTO
                        {
                            Message = $"Concession selected not found.",
                            StatusCode = 404
                        });
                    }

                    var concessionOrderDetail = new ConcessionOrderDetail
                    {
                        ConcessionOrderId = concessionOrder.Id,
                        ConcessionId = concessionItem.Id,
                        Quantity = concession.Quantity,
                        Price = concessionItem.Price * concession.Quantity
                    };

                    // save concession order detail
                    await _unitOfWork.ConcessionOrderDetail.AddAsync(concessionOrderDetail);
                    await _unitOfWork.SaveAsync();
                }
            }

            // create payment
            var payment = new Payment
            {
                EmployeeId = bookingRequest.UserId,
                BookingId = booking.Id,
                ConcessionOrderId = concessionOrder?.Id,
                Email = bookingRequest.Email,
                Name = bookingRequest.Username,
                PhoneNumber = bookingRequest.Phone,
                Amount = booking.TotalAmount + (concessionOrder?.TotalPrice ?? 0),
                PaymentMethod = Constants.PaymentMethod_VnPay,
                PaymentDate = DateTime.Now,
                PaymentStatus = Constants.PaymentStatus_Success
            };

            // save payment
            await _unitOfWork.Payment.AddAsync(payment);
            await _unitOfWork.SaveAsync();

            // send ticket email to customer
            var barcodeText = $"CineMax_Ticket_{payment.Id}_{DateTime.Now:yyyyMMddHHmmss}";
            var barcodeBytes = await _qrCodeService.GenerateQRCodeAsync(barcodeText, 300, 300);
            await _emailService.SendEmailWithAttachmentAsync(payment.Email,
                    "🎟 Your CineMax Ticket Confirmation",
                    HtmlContent.GetTicketEmailHtml(barcodeText, $"{Convert.ToBase64String(barcodeBytes)}"),
                    barcodeBytes,
                    $"{barcodeText}.png");


            return Ok(new SuccessResponseDTO
            {
                Data = new
                {
                    PaymentId = payment.Id,
                },
                Message = "Booking created successfully.",
            });

        }

        [HttpPost("checkin")]
        [Authorize(Roles = Constants.Role_Employee)]
        public async Task<IActionResult> CheckInBooking([FromBody] CheckInBookingRequestDTO request)
        {
            // find payment by id
            var payment = await _unitOfWork.Payment.GetOneAsync(p => p.Id == request.PaymentId && p.PaymentStatus == Constants.PaymentStatus_Success);
            if (payment == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Payment not found or not successful.",
                    StatusCode = 404
                });
            }
            var booking = await _unitOfWork.Booking.GetOneAsync(b => b.Id == payment.BookingId);
            if (booking == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Booking not found.",
                    StatusCode = 404
                });
            }
            // check if booking is already checked in
            if (booking.BookingStatus == Constants.BookingStatus_CheckedIn)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Booking has already been checked in.",
                    StatusCode = 400
                });
            }
            // update booking status to checked in
            booking.BookingStatus = Constants.BookingStatus_CheckedIn;
            booking.CheckedInBy = request.EmployeeId;
            booking.LastUpdatedAt = DateTime.Now;
            // save changes
            _unitOfWork.Booking.Update(booking);
            await _unitOfWork.SaveAsync();

            // send email notification for user
            await _emailService.SendEmailAsync(payment.Email,
                "Check in successfull", "<p>Your have checked in successfully</p>");

            return Ok(new SuccessResponseDTO
            {
                Message = "Booking checked in successfully.",
                Data = new
                {
                    PaymentId = payment.Id,
                    BookingId = booking.Id,
                }
            });
        }
    }
}
