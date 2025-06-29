using CinemaxAPI.Models.DTO;

namespace CinemaxAPI.Middlewares
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var errorResponse = new ErrorResponseDTO
                {
                    Message = "An unexpected error occurred",
                    Errors = ex.Message,
                    StatusCode = context.Response.StatusCode,
                    Status = "Error"
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
