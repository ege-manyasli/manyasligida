using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models.DTOs;

// Request DTOs
public record SalesReportRequest
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? CategoryId { get; init; }
    public int? ProductId { get; init; }
    public string? Period { get; init; } // daily, weekly, monthly, yearly
}

public record RevenueAnalysisRequest
{
    [Required]
    public DateTime StartDate { get; init; }
    
    [Required]
    public DateTime EndDate { get; init; }
    
    public string GroupBy { get; init; } = "month"; // day, week, month, quarter, year
}

public record ExpenseRequest
{
    [Required(ErrorMessage = "Açıklama gereklidir")]
    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olmalıdır")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tutar gereklidir")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
    public decimal Amount { get; init; }

    [Required(ErrorMessage = "Kategori gereklidir")]
    public string Category { get; init; } = string.Empty;

    public DateTime? Date { get; init; }
}

// Response DTOs
public record SalesReportResponse
{
    public decimal TotalRevenue { get; init; }
    public int TotalOrders { get; init; }
    public int TotalItems { get; init; }
    public decimal AverageOrderValue { get; init; }
    public List<SalesByPeriodResponse> SalesByPeriod { get; init; } = new();
    public List<ProductSalesResponse> TopProducts { get; init; } = new();
    public List<CategorySalesResponse> SalesByCategory { get; init; } = new();
}

public record SalesByPeriodResponse
{
    public string Period { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public decimal Revenue { get; init; }
    public int OrderCount { get; init; }
    public int ItemCount { get; init; }
}

public record ProductSalesResponse
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int QuantitySold { get; init; }
    public decimal Revenue { get; init; }
    public decimal Percentage { get; init; }
}

public record CategorySalesResponse
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int QuantitySold { get; init; }
    public decimal Revenue { get; init; }
    public decimal Percentage { get; init; }
}

public record RevenueAnalysisResponse
{
    public decimal TotalRevenue { get; init; }
    public decimal AverageRevenue { get; init; }
    public decimal GrowthRate { get; init; }
    public List<RevenueByPeriodResponse> RevenueByPeriod { get; init; } = new();
    public RevenueTrendResponse Trend { get; init; } = new();
}

public record RevenueByPeriodResponse
{
    public string Period { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal Revenue { get; init; }
    public decimal GrowthRate { get; init; }
}

public record RevenueTrendResponse
{
    public string Direction { get; init; } = string.Empty; // "increasing", "decreasing", "stable"
    public decimal ChangePercentage { get; init; }
    public string Description { get; init; } = string.Empty;
}

public record FinancialSummaryResponse
{
    public decimal TotalRevenue { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetIncome { get; init; }
    public decimal ProfitMargin { get; init; }
    public int TotalOrders { get; init; }
    public decimal AverageOrderValue { get; init; }
    public DateTime ReportDate { get; init; }
    public string Period { get; init; } = string.Empty;
}

public record DashboardMetricsResponse
{
    public decimal TodayRevenue { get; init; }
    public decimal MonthRevenue { get; init; }
    public decimal YearRevenue { get; init; }
    public int TodayOrders { get; init; }
    public int MonthOrders { get; init; }
    public int YearOrders { get; init; }
    public int ActiveCustomers { get; init; }
    public int TotalProducts { get; init; }
    public List<RecentSaleResponse> RecentSales { get; init; } = new();
}

public record RecentSaleResponse
{
    public int OrderId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; } = string.Empty;
}

// Expenses
public record ExpenseResponse
{
    public int Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime ExpenseDate { get; init; }
    public DateTime CreatedAt { get; init; }
}