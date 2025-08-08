namespace manyasligida.Services
{
    public interface IPerformanceMonitorService
    {
        void LogRequestTime(string path, long elapsedMilliseconds);
        void LogDatabaseQuery(string query, long elapsedMilliseconds);
        void LogError(string error, string context);
        Dictionary<string, object> GetPerformanceMetrics();
    }
}
