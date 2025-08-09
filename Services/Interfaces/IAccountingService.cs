using manyasligida.Models.DTOs;

namespace manyasligida.Services.Interfaces;

public interface IAccountingService
{
    // Sales Reports
    Task<ApiResponse<SalesReportResponse>> GenerateSalesReportAsync(SalesReportRequest request);
    Task<ApiResponse<List<ProductSalesResponse>>> GetTopSellingProductsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<CategorySalesResponse>>> GetSalesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Revenue Analysis
    Task<ApiResponse<RevenueAnalysisResponse>> GetRevenueAnalysisAsync(RevenueAnalysisRequest request);
    Task<ApiResponse<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<decimal>> GetMonthlyGrowthRateAsync();
    Task<ApiResponse<List<RevenueByPeriodResponse>>> GetRevenueByPeriodAsync(string period, int count = 12);
    
    // Expenses Management
    Task<ApiResponse<bool>> CreateExpenseAsync(ExpenseRequest request);
    Task<ApiResponse<bool>> UpdateExpenseAsync(int expenseId, ExpenseRequest request);
    Task<ApiResponse<bool>> DeleteExpenseAsync(int expenseId);
    Task<ApiResponse<List<object>>> GetExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<decimal>> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<Dictionary<string, decimal>>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Financial Reports
    Task<ApiResponse<FinancialSummaryResponse>> GetFinancialSummaryAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<DashboardMetricsResponse>> GetDashboardMetricsAsync();
    Task<ApiResponse<bool>> GenerateFinancialReportAsync(string reportType, DateTime startDate, DateTime endDate);
    Task<ApiResponse<List<object>>> GetFinancialReportsAsync(int page = 1, int pageSize = 20);
    
    // Analytics & Insights
    Task<ApiResponse<object>> GetProfitabilityAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<object>>> GetCustomerValueAnalysisAsync();
    Task<ApiResponse<object>> GetSeasonalTrendsAsync();
    Task<ApiResponse<List<object>>> GetInventoryTurnoverAsync();
    
    // Data Export
    Task<ApiResponse<byte[]>> ExportSalesReportAsync(SalesReportRequest request, string format = "excel");
    Task<ApiResponse<byte[]>> ExportFinancialReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
    
    // Automated Tasks
    Task<ApiResponse<bool>> ProcessDailyRevenueAsync();
    Task<ApiResponse<bool>> UpdateProductAnalyticsAsync();
    Task<ApiResponse<bool>> CleanupOldAnalyticsAsync(int daysToKeep = 365);
}
