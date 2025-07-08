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

namespace CinemaxAPI.Controllers.Manager
{
    [Route("api/movies")]
    [ApiController]
    //[Authorize(Roles = Constants.Role_Manager)]
    public class MovieController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public MovieController(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies([FromQuery] MovieFilterRequestDTO filter, [FromQuery] PagedRequestDTO paged)
        {
            var (movies, totalCount) = await _unitOfWork.Movie.GetAllAsync(filter, paged);
            if (movies == null || !movies.Any())
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<MovieDTO>(),
                    Message = "No movies found."
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Data = new
                {
                    Movies = _mapper.Map<List<MovieDTO>>(movies),
                    TotalCount = totalCount,
                    TotalResult = movies.Count(),
                    Page = paged?.PageNumber > 0 ? paged.PageNumber : 1
                },
                Message = "Movies retrieved successfully."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(int id)
        {
            var movie = await _unitOfWork.Movie.GetOneAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Movie not found.",
                    StatusCode = 404
                });
            }
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<MovieDTO>(movie),
                Message = "Movie retrieved successfully."
            });
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreateMovie([FromForm] CreateMovieRequestDTO request)
        {
            var newMovie = new Movie
            {
                Title = request.Title,
                Genre = request.Genre,
                Director = request.Director,
                Cast = request.Cast,
                Description = request.Description,
                Duration = request.Duration,
                ReleaseDate = request.ReleaseDate,
                TrailerUrl = request.TrailerUrl,
            };

            // save image if provided
            IFormFile file = request.File;

            if (file != null)
            {
                ValidateImage(file);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Invalid file",
                        StatusCode = 400,
                        Status = "Error"
                    });
                }

                var newFileName = await _imageService.Upload(file);
                if (string.IsNullOrEmpty(newFileName))
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Failed to upload image.",
                        StatusCode = 500,
                        Status = "Error"
                    });
                }

                newMovie.PosterUrl = newFileName;
            }

            await _unitOfWork.Movie.AddAsync(newMovie);
            await _unitOfWork.SaveAsync();
            var movieDto = _mapper.Map<MovieDTO>(newMovie);
            return CreatedAtAction(nameof(GetMovieById), new { id = newMovie.Id }, new SuccessResponseDTO
            {
                Data = movieDto,
                Message = "Movie created successfully."
            });
        }

        private void ValidateImage(IFormFile file)
        {
            // check for max file size (5Mb)
            if (file.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("File", "File size exceeds the maximum limit of 5MB.");
            }

            // check for correct extension (jpg, png, jpeg)
            var validExtenstions = new string[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName);

            if (!validExtenstions.Contains(fileExtension.ToLower()))
            {
                ModelState.AddModelError("File", "Unsupported file extension. Only .jpg, .jpeg, and .png are allowed.");
            }
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateMovie(int id, [FromForm] UpdateMovieRequestDTO request)
        {
            var movie = await _unitOfWork.Movie.GetOneAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Movie not found.",
                    StatusCode = 404
                });
            }

            // update movie properties
            movie.Title = request.Title;
            movie.Genre = request.Genre;
            movie.Director = request.Director;
            movie.Cast = request.Cast;
            movie.Description = request.Description;
            movie.Duration = request.Duration;
            movie.ReleaseDate = request.ReleaseDate;
            movie.TrailerUrl = request.TrailerUrl;


            // save image if provided
            IFormFile file = request.File;

            if (file != null)
            {
                ValidateImage(file);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Invalid file",
                        StatusCode = 400,
                        Status = "Error"
                    });
                }

                var newFileName = await _imageService.Upload(file);
                if (string.IsNullOrEmpty(newFileName))
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Failed to upload image.",
                        StatusCode = 500,
                        Status = "Error"
                    });
                }

                movie.PosterUrl = newFileName;
            }

            movie.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Movie.Update(movie);
            await _unitOfWork.SaveAsync();
            var movieDto = _mapper.Map<MovieDTO>(movie);
            return Ok(new SuccessResponseDTO
            {
                Data = movieDto,
                Message = "Movie updated successfully."
            });
        }

        [HttpPut("{id}/disable")]
        [Authorize(Roles = $"{Constants.Role_Manager}")]
        public async Task<IActionResult> DisableMovie(int id)
        {
            var movie = await _unitOfWork.Movie.GetOneAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Movie not found.",
                    StatusCode = 404
                });
            }

            movie.IsActive = false;
            movie.LastUpdatedAt = DateTime.Now;

            _unitOfWork.Movie.Update(movie);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = movie.Id,
                Message = "Movie disabled successfully."
            });
        }

        [HttpPut("{id}/enable")]
        [Authorize(Roles = $"{Constants.Role_Manager}")]
        public async Task<IActionResult> EnableMovie(int id)
        {
            var movie = await _unitOfWork.Movie.GetOneAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Movie not found.",
                    StatusCode = 404
                });
            }

            movie.IsActive = true;
            movie.LastUpdatedAt = DateTime.Now;

            _unitOfWork.Movie.Update(movie);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = movie.Id,
                Message = "Movie enabled successfully."
            });
        }
    }
}
