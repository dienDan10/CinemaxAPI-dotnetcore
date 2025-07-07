using AutoMapper;
using CinemaxAPI.CustomActionFilters;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CinemaxAPI.Controllers.Manager
{
    [Route("api/movies")]
    [ApiController]
    [Authorize(Roles = Constants.Role_Manager)]
    public class MovieController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MovieController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            var movies = await _unitOfWork.Movie.GetAllAsync();
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
                Data = _mapper.Map<List<MovieDTO>>(movies),
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
        public async Task<IActionResult> CreateMovie([FromBody] CreateMovieRequestDTO request)
        {
            var movie = _mapper.Map<Movie>(request);
            movie.CreatedAt = DateTime.Now;
            movie.LastUpdatedAt = DateTime.Now;
            movie.IsActive = true;
            await _unitOfWork.Movie.AddAsync(movie);
            await _unitOfWork.SaveAsync();
            // Only map properties present in MovieDTO
            var movieDto = _mapper.Map<MovieDTO>(movie);
            return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, new SuccessResponseDTO
            {
                Data = movieDto,
                Message = "Movie created successfully."
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] UpdateMovieRequestDTO request)
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
            // Update fields
            movie.Title = request.Title;
            movie.Genre = request.Genre;
            movie.Director = request.Director;
            movie.Cast = request.Cast;
            movie.Description = request.Description;
            movie.Duration = request.Duration;
            movie.ReleaseDate = request.ReleaseDate;
            movie.PosterUrl = request.PosterUrl;
            movie.TrailerUrl = request.TrailerUrl;
            movie.IsActive = request.IsActive;
            movie.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Movie.Update(movie);
            await _unitOfWork.SaveAsync();
            // Only map properties present in MovieDTO
            var movieDto = _mapper.Map<MovieDTO>(movie);
            return Ok(new SuccessResponseDTO
            {
                Data = movieDto,
                Message = "Movie updated successfully."
            });
        }

        [HttpPost("upload-poster")]
        [RequestSizeLimit(10_000_000)] // Limit to 10MB
        public async Task<IActionResult> UploadPoster([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "No file uploaded.",
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileExt = Path.GetExtension(file.FileName);
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowedExts.Contains(fileExt.ToLower()))
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Invalid file type.",
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            var uniqueFileName = $"poster_{Guid.NewGuid()}{fileExt}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var imageUrl = $"{baseUrl}/images/{uniqueFileName}";

            return Ok(new SuccessResponseDTO
            {
                Message = "Image uploaded successfully.",
                Data = new { imageUrl }
            });
        }
    }
}
