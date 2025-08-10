using Microsoft.AspNetCore.Mvc;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;
using manyasligida.Services;
using manyasligida.Attributes;

namespace manyasligida.Controllers;

[Route("Accounting")]
[AdminAuthorization]
public class AccountingController : Controller
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<AccountingController> _logger;

    public AccountingController(IAccountingService accountingService, ILogger<AccountingController> logger)
    {
        _accountingService = accountingService;
        _logger = logger;
    }

    #region Dashboard & Overview

    [HttpGet]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var metricsResult = await _accountingService.GetDashboardMetricsAsync();
            
            if (metricsResult.Success)
            {
                return View(metricsResult.Data);
            }

            TempData["Error"] = metricsResult.Message;
            return View(new DashboardMetricsResponse());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading accounting dashboard");
            TempData["Error"] = "Dashboard yüklenirken bir hata oluştu";
            return View(new DashboardMetricsResponse());
        }
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        return await Index();
    }

    #endregion

    #region Sales Reports

    [HttpGet("Sales")]
    public IActionResult Sales()
    {
        var request = new SalesReportRequest
        {
            StartDate = DateTimeHelper.TodayTurkey.AddMonths(-1),
            EndDate = DateTimeHelper.TodayTurkey,
            Period = "monthly"
        };

        return View(request);
    }

    [HttpPost("Sales/Generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSalesReport(SalesReportRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View("Sales", request);
            }

            var result = await _accountingService.GenerateSalesReportAsync(request);

            if (result.Success)
            {
                ViewBag.ReportData = result.Data;
                ViewBag.Request = request;
                return View("SalesReport", result.Data);
            }

            TempData["Error"] = result.Message;
            return View("Sales", request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales report");
            TempData["Error"] = "Satış raporu oluşturulurken bir hata oluştu";
            return View("Sales", request);
        }
    }

    [HttpGet("api/sales-report")]
    public async Task<IActionResult> GetSalesReportApi([FromQuery] SalesReportRequest request)
    {
        try
        {
            var result = await _accountingService.GenerateSalesReportAsync(request);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting sales report");
            return Json(ApiResponse<SalesReportResponse>.FailureResult("Satış raporu alınamadı"));
        }
    }

    #endregion

    #region Revenue Analysis

    [HttpGet("Revenue")]
    public IActionResult Revenue()
    {
        var request = new RevenueAnalysisRequest
        {
            StartDate = DateTimeHelper.TodayTurkey.AddMonths(-6),
            EndDate = DateTimeHelper.TodayTurkey,
            GroupBy = "month"
        };

        return View(request);
    }

    [HttpPost("Revenue/Analyze")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnalyzeRevenue(RevenueAnalysisRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View("Revenue", request);
            }

            var result = await _accountingService.GetRevenueAnalysisAsync(request);

            if (result.Success)
            {
                ViewBag.AnalysisData = result.Data;
                ViewBag.Request = request;
                return View("RevenueAnalysis", result.Data);
            }

            TempData["Error"] = result.Message;
            return View("Revenue", request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing revenue");
            TempData["Error"] = "Gelir analizi yapılırken bir hata oluştu";
            return View("Revenue", request);
        }
    }

    [HttpGet("api/revenue-analysis")]
    public async Task<IActionResult> GetRevenueAnalysisApi([FromQuery] RevenueAnalysisRequest request)
    {
        try
        {
            var result = await _accountingService.GetRevenueAnalysisAsync(request);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting revenue analysis");
            return Json(ApiResponse<RevenueAnalysisResponse>.FailureResult("Gelir analizi alınamadı"));
        }
    }

    #endregion

    #region Expenses Management

    [HttpGet("Expenses")]
    public async Task<IActionResult> Expenses()
    {
        try
        {
            var expensesResult = await _accountingService.GetExpensesAsync();
            
            if (expensesResult.Success)
            {
                ViewBag.Expenses = expensesResult.Data;
            }
            else
            {
                ViewBag.Expenses = new List<object>();
                TempData["Error"] = expensesResult.Message;
            }

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading expenses");
            TempData["Error"] = "Giderler yüklenirken bir hata oluştu";
            ViewBag.Expenses = new List<object>();
            return View();
        }
    }

    [HttpPost("Expenses/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExpense(ExpenseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Geçersiz veri", errors });
            }

            var result = await _accountingService.CreateExpenseAsync(request);

            _logger.LogInformation("Expense creation attempt: {Success}, Description: {Description}", 
                result.Success, request.Description);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return Json(new { success = false, message = "Gider oluşturulamadı" });
        }
    }

    [HttpPost("Expenses/Update/{expenseId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateExpense(int expenseId, ExpenseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz veri" });
            }

            var result = await _accountingService.UpdateExpenseAsync(expenseId, request);

            _logger.LogInformation("Expense update attempt: {Success}, ExpenseId: {ExpenseId}", 
                result.Success, expenseId);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense");
            return Json(new { success = false, message = "Gider güncellenemedi" });
        }
    }

    [HttpPost("Expenses/Delete/{expenseId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExpense(int expenseId)
    {
        try
        {
            var result = await _accountingService.DeleteExpenseAsync(expenseId);

            _logger.LogInformation("Expense deletion attempt: {Success}, ExpenseId: {ExpenseId}", 
                result.Success, expenseId);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense");
            return Json(new { success = false, message = "Gider silinemedi" });
        }
    }

    [HttpGet("api/expenses")]
    public async Task<IActionResult> GetExpensesApi(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var result = await _accountingService.GetExpensesAsync(startDate, endDate);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting expenses");
            return Json(ApiResponse<List<object>>.FailureResult("Giderler alınamadı"));
        }
    }

    #endregion

    #region Financial Summary & Reports

    [HttpGet("Financial")]
    public IActionResult Financial()
    {
        var startDate = DateTime.UtcNow.AddMonths(-1).Date;
        var endDate = DateTime.UtcNow.Date;

        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;

        return View();
    }

    [HttpPost("Financial/Summary")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetFinancialSummary(DateTime startDate, DateTime endDate)
    {
        try
        {
            var result = await _accountingService.GetFinancialSummaryAsync(startDate, endDate);

            if (result.Success)
            {
                ViewBag.Summary = result.Data;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                return View("FinancialSummary", result.Data);
            }

            TempData["Error"] = result.Message;
            return RedirectToAction("Financial");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial summary");
            TempData["Error"] = "Mali özet alınırken bir hata oluştu";
            return RedirectToAction("Financial");
        }
    }

    [HttpGet("api/financial-summary")]
    public async Task<IActionResult> GetFinancialSummaryApi(DateTime startDate, DateTime endDate)
    {
        try
        {
            var result = await _accountingService.GetFinancialSummaryAsync(startDate, endDate);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting financial summary");
            return Json(ApiResponse<FinancialSummaryResponse>.FailureResult("Mali özet alınamadı"));
        }
    }

    #endregion

    #region Analytics & Insights

    [HttpGet("Analytics")]
    public IActionResult Analytics()
    {
        return View();
    }

    [HttpGet("api/dashboard-metrics")]
    public async Task<IActionResult> GetDashboardMetricsApi()
    {
        try
        {
            var result = await _accountingService.GetDashboardMetricsAsync();
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting dashboard metrics");
            return Json(ApiResponse<DashboardMetricsResponse>.FailureResult("Dashboard metrikleri alınamadı"));
        }
    }

    [HttpGet("api/top-products")]
    public async Task<IActionResult> GetTopProductsApi(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var result = await _accountingService.GetTopSellingProductsAsync(count, startDate, endDate);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting top products");
            return Json(ApiResponse<List<ProductSalesResponse>>.FailureResult("En çok satan ürünler alınamadı"));
        }
    }

    [HttpGet("api/sales-by-category")]
    public async Task<IActionResult> GetSalesByCategoryApi(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var result = await _accountingService.GetSalesByCategoryAsync(startDate, endDate);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting sales by category");
            return Json(ApiResponse<List<CategorySalesResponse>>.FailureResult("Kategori satışları alınamadı"));
        }
    }

    [HttpGet("api/revenue-trends")]
    public async Task<IActionResult> GetRevenueTrendsApi(string period = "month", int count = 12)
    {
        try
        {
            var result = await _accountingService.GetRevenueByPeriodAsync(period, count);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting revenue trends");
            return Json(ApiResponse<List<RevenueByPeriodResponse>>.FailureResult("Gelir trendleri alınamadı"));
        }
    }

    [HttpGet("api/expenses-by-category")]
    public async Task<IActionResult> GetExpensesByCategoryApi(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var result = await _accountingService.GetExpensesByCategoryAsync(startDate, endDate);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting expenses by category");
            return Json(ApiResponse<Dictionary<string, decimal>>.FailureResult("Kategori giderleri alınamadı"));
        }
    }

    #endregion

    #region Data Export

    [HttpPost("Export/Sales")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportSalesReport(SalesReportRequest request, string format = "excel")
    {
        try
        {
            var result = await _accountingService.ExportSalesReportAsync(request, format);

            if (result.Success && result.Data != null)
            {
                var contentType = format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => "application/octet-stream"
                };

                var fileName = $"SalesReport_{DateTime.UtcNow:yyyyMMdd}.{format}";
                return File(result.Data, contentType, fileName);
            }

            TempData["Error"] = result.Message;
            return RedirectToAction("Sales");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting sales report");
            TempData["Error"] = "Rapor dışa aktarılamadı";
            return RedirectToAction("Sales");
        }
    }

    [HttpPost("Export/Financial")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportFinancialReport(DateTime startDate, DateTime endDate, string format = "pdf")
    {
        try
        {
            var result = await _accountingService.ExportFinancialReportAsync(startDate, endDate, format);

            if (result.Success && result.Data != null)
            {
                var contentType = format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => "application/octet-stream"
                };

                var fileName = $"FinancialReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";
                return File(result.Data, contentType, fileName);
            }

            TempData["Error"] = result.Message;
            return RedirectToAction("Financial");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting financial report");
            TempData["Error"] = "Mali rapor dışa aktarılamadı";
            return RedirectToAction("Financial");
        }
    }

    #endregion

    #region Background Tasks

    [HttpPost("Tasks/ProcessDailyRevenue")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessDailyRevenue()
    {
        try
        {
            var result = await _accountingService.ProcessDailyRevenueAsync();

            _logger.LogInformation("Daily revenue processing: {Success}", result.Success);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing daily revenue");
            return Json(new { success = false, message = "Günlük gelir işlemi başarısız" });
        }
    }

    [HttpPost("Tasks/UpdateAnalytics")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAnalytics()
    {
        try
        {
            var result = await _accountingService.UpdateProductAnalyticsAsync();

            _logger.LogInformation("Analytics update: {Success}", result.Success);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating analytics");
            return Json(new { success = false, message = "Analitik güncelleme başarısız" });
        }
    }

    [HttpPost("Tasks/CleanupOldData")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleanupOldData(int daysToKeep = 365)
    {
        try
        {
            var result = await _accountingService.CleanupOldAnalyticsAsync(daysToKeep);

            _logger.LogInformation("Old data cleanup: {Success}, Days: {Days}", result.Success, daysToKeep);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old data");
            return Json(new { success = false, message = "Eski veri temizleme başarısız" });
        }
    }

    #endregion
}
