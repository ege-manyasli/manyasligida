using manyasligida.Models.DTOs;

namespace manyasligida.Services.Interfaces
{
    public interface IInventoryService
    {
        // Mal alımı işlemleri
        Task<ApiResponse<InventoryDto>> AddPurchaseAsync(InventoryInput input);
        Task<ApiResponse<List<InventoryDto>>> GetPurchasesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<InventoryDto>> GetPurchaseByIdAsync(int id);
        Task<ApiResponse<bool>> DeletePurchaseAsync(int id);
        
        // Stok takibi
        Task<ApiResponse<InventoryStockDto>> AddStockItemAsync(InventoryStockInput input);
        Task<ApiResponse<List<InventoryStockDto>>> GetStockItemsAsync();
        Task<ApiResponse<InventoryStockDto>> GetStockItemByNameAsync(string itemName);
        Task<ApiResponse<bool>> UpdateStockAsync(string itemName, decimal quantity, string transactionType);
        Task<ApiResponse<bool>> DeleteStockItemAsync(int id);
        
        // Stok işlemleri
        Task<ApiResponse<InventoryTransactionDto>> AddTransactionAsync(InventoryTransactionInput input);
        Task<ApiResponse<List<InventoryTransactionDto>>> GetTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        // Dashboard ve raporlar
        Task<ApiResponse<InventoryDashboardViewModel>> GetDashboardAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<List<InventoryStockDto>>> GetLowStockItemsAsync();
        Task<ApiResponse<decimal>> GetTotalStockValueAsync();
        
        // Yardımcı metodlar
        Task<ApiResponse<bool>> UpdateAverageCostAsync(string itemName);
        Task<ApiResponse<List<string>>> GetCategoriesAsync();
        Task<ApiResponse<List<string>>> GetSuppliersAsync();
    }
}
