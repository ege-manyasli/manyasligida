using manyasligida.Services;

namespace manyasligida.Middleware
{
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly IPerformanceMonitorService _performanceMonitor;

        public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger, IPerformanceMonitorService performanceMonitor)
        {
            _next = next;
            _logger = logger;
            _performanceMonitor = performanceMonitor;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsed = stopwatch.ElapsedMilliseconds;
                
                // Log performance metrics
                _performanceMonitor.LogRequestTime(context.Request.Path, elapsed);
                
                // Log slow requests
                if (elapsed > 1000)
                {
                    _logger.LogWarning("Slow request: {Method} {Path} took {Time}ms from {IP}", 
                        context.Request.Method, 
                        context.Request.Path, 
                        elapsed,
                        context.Connection.RemoteIpAddress);
                }
                
                // Add performance header for debugging only if response hasn't started
                if (!context.Response.HasStarted)
                {
                    context.Response.Headers.Append("X-Response-Time", $"{elapsed}ms");
                }
            }
        }
    }
}
