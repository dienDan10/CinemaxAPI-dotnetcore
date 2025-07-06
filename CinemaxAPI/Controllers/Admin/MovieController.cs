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

namespace CinemaxAPI.Controllers.Admin
{
    [Route("api/movies")]
    [ApiController]
    [Authorize(Roles = Constants.Role_Admin)]
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
            return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, new SuccessResponseDTO
            {
                Data = _mapper.Map<MovieDTO>(movie),
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
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<MovieDTO>(movie),
                Message = "Movie updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
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
            _unitOfWork.Movie.Remove(movie);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Message = "Movie deleted successfully."
            });
        }
    }
}
