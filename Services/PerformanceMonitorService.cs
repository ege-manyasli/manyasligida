using Microsoft.Extensions.Logging;
using System.Linq;

namespace manyasligida.Services
{
    public class PerformanceMonitorService : IPerformanceMonitorService
    {
        private readonly ILogger<PerformanceMonitorService> _logger;
        private readonly Dictionary<string, long> _requestTimes;
        private readonly Dictionary<string, long> _databaseQueries;
        private readonly List<string> _errors;

        public PerformanceMonitorService(ILogger<PerformanceMonitorService> logger)
        {
            _logger = logger;
            _requestTimes = new Dictionary<string, long>();
            _databaseQueries = new Dictionary<string, long>();
            _errors = new List<string>();
        }

        public void LogRequestTime(string path, long elapsedMilliseconds)
        {
            if (_requestTimes.ContainsKey(path))
            {
                _requestTimes[path] = (_requestTimes[path] + elapsedMilliseconds) / 2; // Average
            }
            else
            {
                _requestTimes[path] = elapsedMilliseconds;
            }

            if (elapsedMilliseconds > 1000) // Log slow requests
            {
                _logger.LogWarning("Slow request detected: {Path} took {Time}ms", path, elapsedMilliseconds);
            }
        }

        public void LogDatabaseQuery(string query, long elapsedMilliseconds)
        {
            var queryKey = query.Length > 50 ? query.Substring(0, 50) + "..." : query;
            
            if (_databaseQueries.ContainsKey(queryKey))
            {
                _databaseQueries[queryKey] = (_databaseQueries[queryKey] + elapsedMilliseconds) / 2;
            }
            else
            {
                _databaseQueries[queryKey] = elapsedMilliseconds;
            }

            if (elapsedMilliseconds > 500) // Log slow queries
            {
                _logger.LogWarning("Slow database query detected: {Query} took {Time}ms", queryKey, elapsedMilliseconds);
            }
        }

        public void LogError(string error, string context)
        {
            _errors.Add($"{DateTime.UtcNow}: {context} - {error}");
            _logger.LogError("Performance error in {Context}: {Error}", context, error);
        }

        public Dictionary<string, object> GetPerformanceMetrics()
        {
            return new Dictionary<string, object>
            {
                ["AverageRequestTimes"] = _requestTimes,
                ["AverageQueryTimes"] = _databaseQueries,
                ["RecentErrors"] = _errors.TakeLast(10).ToList(),
                ["TotalRequests"] = _requestTimes.Count,
                ["TotalQueries"] = _databaseQueries.Count,
                ["TotalErrors"] = _errors.Count
            };
        }
    }
}
