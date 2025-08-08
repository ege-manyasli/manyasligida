namespace manyasligida.Models
{
    public class AccountingDashboardViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Revenue { get; set; }
        public decimal PurchasesTotal { get; set; }
        public decimal ExpensesTotal { get; set; }
        public decimal NetProfit { get; set; }
        public List<PurchaseDto> Purchases { get; set; } = new();
        public List<ExpenseDto> Expenses { get; set; } = new();
    }

    public class PurchaseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Supplier { get; set; }
        public string? Item { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
        public decimal Total => Math.Round(Quantity * UnitPrice, 2);
    }

    public class ExpenseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class PurchaseInput
    {
        public DateTime Date { get; set; }
        public string? Supplier { get; set; }
        public string? Item { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
    }

    public class ExpenseInput
    {
        public DateTime Date { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
    }
}

