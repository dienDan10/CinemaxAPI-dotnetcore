using AutoMapper;
using CinemaxAPI.CustomActionFilters;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Services;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;

namespace CinemaxAPI.Controllers.Customer
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IVNPayService _vnPayService;
        private readonly VNPaySettings _vnPaySettings;
        private readonly IEmailService _emailService;
        private readonly IQRCodeService _qrCodeService;

        public BookingController(IUnitOfWork unitOfWork, IMapper mapper, IVNPayService vnPayService, IOptions<VNPaySettings> vnPayOptions, IEmailService emailService, IQRCodeService qrCodeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _vnPayService = vnPayService;
            _vnPaySettings = vnPayOptions.Value;
            _emailService = emailService;
            _qrCodeService = qrCodeService;
        }

        // get showtime details includes theater, movie, concession, seats
        [HttpGet("showtimes/{id}")]
        [Authorize(Roles = $"{Constants.Role_Customer},{Constants.Role_Employee}")]
        public async Task<IActionResult> GetShowtimeDetail(int id)
        {
            // get showtimes
            var showtime = await _unitOfWork.ShowTime.GetOneAsync(st => st.Id == id && st.Date >= DateTime.Now.Date, includeProperties: "Screen,Movie,Screen.Theater");

            if (showtime == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Showtime not found.",
                    StatusCode = 404
                });
            }

            // get all active concessions
            var concessions = await _unitOfWork.Concession.GetAllAsync(c => c.IsActive && !c.IsRemoved);

            // get the all active seats for the showtime
            var seats = await _unitOfWork.Seat.GetAllAsync(s => s.ScreenId == showtime.ScreenId && s.IsActive && !s.IsRemoved);

            // get all booked seats for this movie
            var bookedSeats = await _unitOfWork.Seat.GetBookedSeatsByShowtimeId(showtime.Id);

            var response = new
            {
                ShowTime = _mapper.Map<ShowTimeDTO>(showtime),
                Movie = _mapper.Map<MovieDTO>(showtime.Movie),
                Theater = _mapper.Map<TheaterDTO>(showtime.Screen.Theater),
                Screen = _mapper.Map<ScreenDTO>(showtime.Screen),
                Concessions = _mapper.Map<List<ConcessionDTO>>(concessions),
                Seats = CreateShowtimeSeat(seats, bookedSeats)
            };

            return Ok(new SuccessResponseDTO
            {
                Data = response,
                Message = "Showtime details retrieved successfully.",
            });
        }

        private List<ShowtimeSeatDTO> CreateShowtimeSeat(IEnumerable<Seat> seats, IEnumerable<Seat> bookedSeats)
        {
            var showtimeSeats = new List<ShowtimeSeatDTO>();
            foreach (var seat in seats)
            {
                var isBooked = bookedSeats.Any(bs => bs.Id == seat.Id);
                showtimeSeats.Add(new ShowtimeSeatDTO
                {
                    Id = seat.Id,
                    SeatRow = seat.SeatRow,
                    SeatNumber = seat.SeatNumber,
                    SeatType = seat.SeatType,
                    IsBooked = isBooked
                });
            }

            return showtimeSeats;
        }

        // request for create a booking
        [HttpPost]
        [Authorize(Roles = $"{Constants.Role_Customer},{Constants.Role_Employee}")]
        [ValidateModel]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDTO bookingRequest)
        {
            // check if the seats count exceeds the limit
            if (bookingRequest.Seats.Count > Constants.MaxBookingSeats)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = $"You can only book up to {Constants.MaxBookingSeats} seats at a time.",
                    StatusCode = 400
                });
            }

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

            // check if user has enough points to use
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (bookingRequest.PointsUsed > 0)
            {
                var user = await _unitOfWork.ApplicationUser.GetOneAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound(new ErrorResponseDTO
                    {
                        Message = "User not found.",
                        StatusCode = 404
                    });
                }
                if (user.Point < bookingRequest.PointsUsed)
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "You do not have enough points to use.",
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
                BookingStatus = Constants.BookingStatus_Pending,
                IsActive = false
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
                    IsActive = false
                };

                // save concession order
                await _unitOfWork.ConcessionOrder.AddAsync(concessionOrder);
                await _unitOfWork.SaveAsync();

                totalAmount += totalConcessionPrice;

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

            // increase promotion usage count if applicable
            if (bookingRequest.PromotionId > 0)
            {
                var promotion = await _unitOfWork.Promotion.GetOneAsync(p => p.Id == bookingRequest.PromotionId && p.IsActive);
                if (promotion == null)
                {
                    return NotFound(new ErrorResponseDTO
                    {
                        Message = "Promotion not found.",
                        StatusCode = 404
                    });
                }
                // check if promotion is valid for this booking
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (promotion.StartDate <= today && promotion.EndDate >= today)
                {
                    promotion.UsedQuantity++;
                    _unitOfWork.Promotion.Update(promotion);
                    await _unitOfWork.SaveAsync();
                }
                else
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Promotion is not valid for this booking.",
                        StatusCode = 400
                    });
                }
            }

            // create payment
            var payment = new Payment
            {
                UserId = bookingRequest.UserId,
                BookingId = booking.Id,
                ConcessionOrderId = concessionOrder?.Id,
                Email = bookingRequest.Email,
                Name = bookingRequest.Username,
                PhoneNumber = bookingRequest.Phone,
                TotalAmount = bookingRequest.TotalAmount,
                DiscountAmount = bookingRequest.DiscountAmount,
                FinalAmount = bookingRequest.FinalAmount,
                PromotionId = bookingRequest.PromotionId > 0 ? bookingRequest.PromotionId : null,
                BonusPointsUsed = bookingRequest.PointsUsed > 0 ? bookingRequest.PointsUsed : null,
                PaymentMethod = Constants.PaymentMethod_VnPay,
                PaymentDate = DateTime.Now,
                PaymentStatus = Constants.PaymentStatus_Pending
            };

            // save payment
            await _unitOfWork.Payment.AddAsync(payment);
            await _unitOfWork.SaveAsync();

            // get payment url from VNPayService
            string ipAddress = "127.0.0.1";
            string paymentUrl = _vnPayService.CreatePaymentUrl(
                amount: payment.FinalAmount,
                orderInfo: $"Movie Ticket Booking #{payment.Id}",
                ipAddress: ipAddress,
                returnUrl: $"{_vnPaySettings.ReturnUrl}?paymentId={payment.Id}",
                tmnCode: _vnPaySettings.TmnCode,
                hashSecret: _vnPaySettings.HashSecret,
                baseUrl: _vnPaySettings.BaseUrl
                );

            // return payment url to client
            return Ok(new SuccessResponseDTO
            {
                Data = new
                {
                    PaymentId = payment.Id,
                    PaymentUrl = paymentUrl
                },
                Message = "Booking created successfully. Please proceed to payment.",
            });

        }

        // verify vnpay payment response
        [HttpPost("vnpay/verify")]
        [Authorize(Roles = $"{Constants.Role_Customer},{Constants.Role_Employee}")]
        [ValidateModel]
        public async Task<IActionResult> VerifyVNPayReturn([FromBody] VNPayVerifyRequestDTO request)
        {
            var payment = await _unitOfWork.Payment.GetOneAsync(p => p.Id == request.PaymentId, "Booking");
            if (payment == null) return NotFound("Payment not found");

            if (payment.PaymentStatus == Constants.PaymentStatus_Success)
            {
                return Ok(new SuccessResponseDTO
                {
                    Message = "Payment verified successfully.",
                    Data = new
                    {
                        PaymentId = payment.Id,
                    }
                });
            }

            var vnpParams = new SortedDictionary<string, string>(request.VnpParams);
            if (!vnpParams.TryGetValue("vnp_SecureHash", out var secureHash))
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Secure hash not found in response.",
                    StatusCode = 400
                });

            vnpParams.Remove("vnp_SecureHash");

            string signData = string.Join("&", vnpParams.Select(kvp =>
                $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

            string checkSum = _vnPayService.HmacSha512(_vnPaySettings.HashSecret, signData);

            if (!secureHash.Equals(checkSum, StringComparison.InvariantCultureIgnoreCase))
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Payment verification failed. Invalid secure hash.",
                    StatusCode = 400
                });

            // Check response code and status
            if (vnpParams.TryGetValue("vnp_ResponseCode", out var responseCode) &&
                vnpParams.TryGetValue("vnp_TransactionStatus", out var transactionStatus) &&
                responseCode == "00" && transactionStatus == "00")
            {
                _unitOfWork.Payment.UpdateStatus(payment.Id, Constants.PaymentStatus_Success);

                if (payment.BookingId != null)
                {
                    var booking = await _unitOfWork.Booking.GetOneAsync(b => b.Id == payment.BookingId.Value, includeProperties: "BookingDetails");
                    if (booking != null)
                    {
                        booking.BookingStatus = Constants.BookingStatus_Success;
                        booking.IsActive = true;
                        _unitOfWork.Booking.Update(booking);
                        // add points for user base on booked seats
                        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var user = await _unitOfWork.ApplicationUser.GetOneAsync(u => u.Id == userId);
                        if (user != null)
                        {
                            user.Point += booking.BookingDetails.Count * Constants.BonusPointsPerSeat;
                            user.Point -= payment.BonusPointsUsed != null ? payment.BonusPointsUsed.Value : 0;
                        }
                    }

                }

                if (payment.ConcessionOrderId != null)
                {
                    var concessionOrder = await _unitOfWork.ConcessionOrder.GetOneAsync(c => c.Id == payment.ConcessionOrderId.Value);
                    if (concessionOrder != null)
                    {
                        concessionOrder.IsActive = true;
                        _unitOfWork.ConcessionOrder.Update(concessionOrder);
                    }
                }

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
                    Message = "Payment verified successfully.",
                    Data = new
                    {
                        PaymentId = payment.Id,
                    }
                });
            }

            // If payment verification fails
            if (payment.BookingId != null)
            {
                var booking = await _unitOfWork.Booking.GetOneAsync(b => b.Id == payment.BookingId.Value);
                if (booking != null)
                {
                    booking.BookingStatus = Constants.BookingStatus_Failed;
                    _unitOfWork.Booking.Update(booking);
                }
            }
            _unitOfWork.Payment.UpdateStatus(payment.Id, Constants.PaymentStatus_Failed);
            await _unitOfWork.SaveAsync();
            return BadRequest(new ErrorResponseDTO
            {
                Message = "Payment verification failed. Invalid response code or transaction status.",
                StatusCode = 400
            });
        }

        [HttpGet("payment/{id}")]
        //[Authorize(Roles = $"{Constants.Role_Customer},{Constants.Role_Employee}")]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            // get the payment
            var payment = await _unitOfWork.Payment.GetOneAsync(p => p.Id == id, includeProperties: "Booking,ConcessionOrder");

            if (payment == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Payment not found.",
                    StatusCode = 404
                });
            }

            // check if the requester is customer, compare id and user id in payment
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (User.IsInRole(Constants.Role_Customer) && payment.UserId != userId)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "You are not the owner of this transaction.",
                    StatusCode = 400
                });
            }

            // get the selected seats
            var bookingDetails = await _unitOfWork.BookingDetail.GetAllAsync(bd => bd.BookingId == payment.BookingId, includeProperties: "Seat");
            var selectedSeats = bookingDetails.Where(bd => bd.Seat != null)
                .Select(bd => _mapper.Map<SeatDTO>(bd.Seat)).ToList();

            // get the ordered concessions
            var concessionOrderDetails = await _unitOfWork.ConcessionOrderDetail
                .GetAllAsync(cod => cod.ConcessionOrderId == payment.ConcessionOrderId, includeProperties: "Concession");
            var selectedConcessions = concessionOrderDetails.Where(cod => cod.Concession != null)
                .Select(cod => new
                {
                    cod.Concession.Id,
                    cod.Concession.Name,
                    cod.Concession.Price,
                    cod.Quantity,
                }).ToList();

            // get the showtime
            var showtime = await _unitOfWork.ShowTime.GetOneAsync(st => st.Id == payment.Booking.ShowTimeId, includeProperties: "Movie,Screen,Screen.Theater");

            if (showtime == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Showtime not found.",
                    StatusCode = 404
                });
            }

            // get the promotion data
            var promotion = await _unitOfWork.Promotion.GetOneAsync(p => p.Id == payment.PromotionId);

            return Ok(new SuccessResponseDTO
            {
                Data = new
                {
                    Payment = _mapper.Map<PaymentDTO>(payment),
                    Promotion = _mapper.Map<PromotionDTO>(promotion),
                    Booking = _mapper.Map<BookingDTO>(payment.Booking),
                    ShowTime = _mapper.Map<ShowTimeDTO>(showtime),
                    Movie = _mapper.Map<MovieDTO>(showtime.Movie),
                    Theater = _mapper.Map<TheaterDTO>(showtime.Screen.Theater),
                    Screen = _mapper.Map<ScreenDTO>(showtime.Screen),
                    Seats = selectedSeats,
                    Concessions = selectedConcessions
                },
                Message = "Booking details retrieved successfully."
            });

        }

        [HttpGet("history")]
        [Authorize(Roles = $"{Constants.Role_Customer}")]
        public async Task<IActionResult> GetBookings([FromQuery] BookingHistoryFilterRequest request)
        {

            // get user id from claims
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new ErrorResponseDTO
                {
                    Message = "You must be logged in to view your bookings.",
                    StatusCode = 401
                });
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // get all payments by user id, only success status
            var startDate = request.FromDate ?? DateTime.Now;
            var endDate = request.ToDate ?? DateTime.Now.AddDays(30);
            var payments = (await _unitOfWork.Payment.GetAllAsync(
                p => p.UserId == userId && p.PaymentStatus == Constants.PaymentStatus_Success && p.PaymentDate >= startDate && p.PaymentDate.Date <= endDate.Date,
                includeProperties: "Booking,ConcessionOrder"))?.ToList();
            if (payments == null || !payments.Any())
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "No successful bookings found for this user.",
                    StatusCode = 404
                });
            }

            var result = new List<object>();
            foreach (var payment in payments)
            {
                var booking = await _unitOfWork.Booking.GetOneAsync(b => b.Id == payment.BookingId, includeProperties: "ShowTime,ShowTime.Movie");
                if (booking == null || booking.ShowTime == null || booking.ShowTime.Movie == null)
                    continue;

                result.Add(new
                {
                    PaymentId = payment.Id,
                    PaymentStatus = payment.PaymentStatus,
                    PaymentDate = payment.PaymentDate,
                    MovieName = booking.ShowTime.Movie.Title,
                    ShowDate = booking.ShowTime.Date,
                    TotalAmount = payment.Amount
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Data = result,
                Message = "Successful bookings retrieved successfully."
            });
        }
    }
}
