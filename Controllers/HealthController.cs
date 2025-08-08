using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IServiceProvider serviceProvider, ILogger<HealthController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "OK", timestamp = DateTime.UtcNow });
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            try
            {
                // Test database connection
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    await context.Database.CanConnectAsync();
                    var healthStatus = new
                    {
                        status = "OK",
                        timestamp = DateTime.UtcNow,
                        database = "Connected",
                        uptime = Environment.TickCount / 1000.0
                    };
                    return Ok(healthStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database health check failed");
                    var healthStatus = new
                    {
                        status = "Degraded",
                        timestamp = DateTime.UtcNow,
                        database = "Disconnected",
                        uptime = Environment.TickCount / 1000.0,
                        error = ex.Message
                    };
                    return Ok(healthStatus);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { status = "Error", message = ex.Message });
            }
        }
    }
}
