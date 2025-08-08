using System.Net;
using System.Text.Json;

namespace manyasligida.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path} from {IP}", 
                    context.Request.Method, 
                    context.Request.Path,
                    context.Connection.RemoteIpAddress);
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log detailed error information
            _logger.LogError(exception, "Detailed error information for request {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = new
                {
                    message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                    details = _environment.IsDevelopment() ? exception.StackTrace : null,
                    timestamp = DateTime.UtcNow,
                    requestId = context.TraceIdentifier
                }
            };

            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    _logger.LogWarning("Bad request error: {Message}", exception.Message);
                    break;
                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    _logger.LogWarning("Unauthorized access attempt: {Message}", exception.Message);
                    break;
                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    _logger.LogWarning("Resource not found: {Message}", exception.Message);
                    break;
                case InvalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    _logger.LogWarning("Invalid operation: {Message}", exception.Message);
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    _logger.LogError(exception, "Internal server error occurred");
                    break;
            }

            // AJAX istekleri için JSON döndür
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                context.Request.ContentType?.Contains("application/json") == true ||
                context.Request.Path.StartsWithSegments("/api"))
            {
                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
            }
            else
            {
                // Normal HTTP istekleri için hata sayfasına yönlendir
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}