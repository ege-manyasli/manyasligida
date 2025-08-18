namespace manyasligida.Models.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Supplier { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class InventoryStockDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public string? Unit { get; set; }
        public decimal AverageCost { get; set; }
        public decimal StockValue { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsLowStock => CurrentStock <= MinimumStock;
    }
    
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    
    public class InventoryInput
    {
        public string ItemName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Supplier { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Notes { get; set; }
    }
    
    public class InventoryStockInput
    {
        public string ItemName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal MinimumStock { get; set; }
        public string? Unit { get; set; }
    }
    
    public class InventoryTransactionInput
    {
        public string ItemName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // IN, OUT
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    
    public class InventoryDashboardViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal TotalStockValue { get; set; }
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public List<InventoryDto> RecentPurchases { get; set; } = new();
        public List<InventoryStockDto> StockItems { get; set; } = new();
        public List<InventoryTransactionDto> RecentTransactions { get; set; } = new();
    }
}
