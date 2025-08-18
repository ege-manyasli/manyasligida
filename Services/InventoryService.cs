using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;
using manyasligida.Services;

namespace manyasligida.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Mal Alımı İşlemleri

        public async Task<ApiResponse<InventoryDto>> AddPurchaseAsync(InventoryInput input)
        {
            try
            {
                var inventory = new Inventory
                {
                    ItemName = input.ItemName,
                    Category = input.Category,
                    UnitPrice = input.UnitPrice,
                    Quantity = input.Quantity,
                    Unit = input.Unit,
                    Supplier = input.Supplier,
                    PurchaseDate = input.PurchaseDate,
                    Notes = input.Notes
                };

                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                // Stok güncelleme
                await UpdateStockFromPurchaseAsync(inventory);

                var dto = MapToInventoryDto(inventory);
                return ApiResponse<InventoryDto>.SuccessResult(dto, "Mal alımı başarıyla kaydedildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory purchase");
                return ApiResponse<InventoryDto>.FailureResult("Mal alımı kaydedilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<List<InventoryDto>>> GetPurchasesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Inventories.Where(i => i.IsActive);

                if (startDate.HasValue)
                    query = query.Where(i => i.PurchaseDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(i => i.PurchaseDate <= endDate.Value);

                var purchases = await query
                    .OrderByDescending(i => i.PurchaseDate)
                    .ToListAsync();

                var dtos = purchases.Select(MapToInventoryDto).ToList();
                return ApiResponse<List<InventoryDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory purchases");
                return ApiResponse<List<InventoryDto>>.FailureResult("Mal alımları getirilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<InventoryDto>> GetPurchaseByIdAsync(int id)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null)
                    return ApiResponse<InventoryDto>.FailureResult("Mal alımı bulunamadı");

                var dto = MapToInventoryDto(inventory);
                return ApiResponse<InventoryDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory purchase by id: {Id}", id);
                return ApiResponse<InventoryDto>.FailureResult("Mal alımı getirilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> DeletePurchaseAsync(int id)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null)
                    return ApiResponse<bool>.FailureResult("Mal alımı bulunamadı");

                // Soft delete yap - her zaman
                inventory.IsActive = false;
                inventory.UpdatedAt = DateTimeHelper.NowTurkey;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Mal alımı başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory purchase: {Id}", id);
                return ApiResponse<bool>.FailureResult("Mal alımı silinirken hata oluştu: " + ex.Message);
            }
        }

        #endregion

        #region Stok Takibi

        public async Task<ApiResponse<InventoryStockDto>> AddStockItemAsync(InventoryStockInput input)
        {
            try
            {
                var existingStock = await _context.InventoryStocks
                    .FirstOrDefaultAsync(s => s.ItemName == input.ItemName && s.IsActive);

                if (existingStock != null)
                    return ApiResponse<InventoryStockDto>.FailureResult("Bu mal zaten stokta mevcut");

                var stock = new InventoryStock
                {
                    ItemName = input.ItemName,
                    Category = input.Category,
                    MinimumStock = input.MinimumStock,
                    Unit = input.Unit
                };

                _context.InventoryStocks.Add(stock);
                await _context.SaveChangesAsync();

                var dto = MapToInventoryStockDto(stock);
                return ApiResponse<InventoryStockDto>.SuccessResult(dto, "Stok kalemi başarıyla eklendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock item");
                return ApiResponse<InventoryStockDto>.FailureResult("Stok kalemi eklenirken hata oluştu");
            }
        }

        public async Task<ApiResponse<List<InventoryStockDto>>> GetStockItemsAsync()
        {
            try
            {
                var stocks = await _context.InventoryStocks
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.ItemName)
                    .ToListAsync();

                var dtos = stocks.Select(MapToInventoryStockDto).ToList();
                return ApiResponse<List<InventoryStockDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock items");
                return ApiResponse<List<InventoryStockDto>>.FailureResult("Stok kalemleri getirilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<InventoryStockDto>> GetStockItemByNameAsync(string itemName)
        {
            try
            {
                var stock = await _context.InventoryStocks
                    .FirstOrDefaultAsync(s => s.ItemName == itemName && s.IsActive);

                if (stock == null)
                    return ApiResponse<InventoryStockDto>.FailureResult("Stok kalemi bulunamadı");

                var dto = MapToInventoryStockDto(stock);
                return ApiResponse<InventoryStockDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock item by name: {ItemName}", itemName);
                return ApiResponse<InventoryStockDto>.FailureResult("Stok kalemi getirilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> UpdateStockAsync(string itemName, decimal quantity, string transactionType)
        {
            try
            {
                var stock = await _context.InventoryStocks
                    .FirstOrDefaultAsync(s => s.ItemName == itemName && s.IsActive);

                if (stock == null)
                    return ApiResponse<bool>.FailureResult("Stok kalemi bulunamadı");

                if (transactionType == "IN")
                    stock.CurrentStock += quantity;
                else if (transactionType == "OUT")
                {
                    if (stock.CurrentStock < quantity)
                        return ApiResponse<bool>.FailureResult("Yetersiz stok");
                    stock.CurrentStock -= quantity;
                }

                stock.LastUpdated = DateTimeHelper.NowTurkey;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Stok başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock: {ItemName}, {Quantity}, {TransactionType}", itemName, quantity, transactionType);
                return ApiResponse<bool>.FailureResult("Stok güncellenirken hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> DeleteStockItemAsync(int id)
        {
            try
            {
                var stock = await _context.InventoryStocks.FindAsync(id);
                if (stock == null)
                    return ApiResponse<bool>.FailureResult("Stok kalemi bulunamadı");

                // İlişkili kayıtları kontrol et
                var hasTransactions = await _context.InventoryTransactions.AnyAsync(t => t.ItemName == stock.ItemName);
                var hasPurchases = await _context.Inventories.AnyAsync(i => i.ItemName == stock.ItemName && i.IsActive);

                if (hasTransactions || hasPurchases)
                {
                    // Soft delete yap
                    stock.IsActive = false;
                    stock.UpdatedAt = DateTimeHelper.NowTurkey;
                    await _context.SaveChangesAsync();
                    return ApiResponse<bool>.SuccessResult(true, "Stok kalemi başarıyla pasif yapıldı");
                }

                // Hiçbir ilişki yoksa tamamen sil
                _context.InventoryStocks.Remove(stock);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Stok kalemi başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stock item: {Id}", id);
                return ApiResponse<bool>.FailureResult("Stok kalemi silinirken hata oluştu: " + ex.Message);
            }
        }

        #endregion

        #region Stok İşlemleri

        public async Task<ApiResponse<InventoryTransactionDto>> AddTransactionAsync(InventoryTransactionInput input)
        {
            try
            {
                var transaction = new InventoryTransaction
                {
                    ItemName = input.ItemName,
                    TransactionType = input.TransactionType,
                    Quantity = input.Quantity,
                    Unit = input.Unit,
                    UnitPrice = input.UnitPrice,
                    Description = input.Description,
                    TransactionDate = input.TransactionDate
                };

                _context.InventoryTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Stok güncelleme
                await UpdateStockAsync(input.ItemName, input.Quantity, input.TransactionType);

                var dto = MapToInventoryTransactionDto(transaction);
                return ApiResponse<InventoryTransactionDto>.SuccessResult(dto, "İşlem başarıyla kaydedildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory transaction");
                return ApiResponse<InventoryTransactionDto>.FailureResult("İşlem kaydedilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<List<InventoryTransactionDto>>> GetTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.InventoryTransactions.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= endDate.Value);

                var transactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                var dtos = transactions.Select(MapToInventoryTransactionDto).ToList();
                return ApiResponse<List<InventoryTransactionDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory transactions");
                return ApiResponse<List<InventoryTransactionDto>>.FailureResult("İşlemler getirilirken hata oluştu");
            }
        }

        #endregion

        #region Dashboard ve Raporlar

        public async Task<ApiResponse<InventoryDashboardViewModel>> GetDashboardAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var purchases = await GetPurchasesAsync(startDate, endDate);
                var stocks = await GetStockItemsAsync();
                var transactions = await GetTransactionsAsync(startDate, endDate);
                var lowStockItems = await GetLowStockItemsAsync();
                var totalStockValue = await GetTotalStockValueAsync();

                var dashboard = new InventoryDashboardViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPurchaseAmount = purchases.Data?.Sum(p => p.TotalAmount) ?? 0,
                    TotalStockValue = totalStockValue.Data,
                    TotalItems = stocks.Data?.Count ?? 0,
                    LowStockItems = lowStockItems.Data?.Count ?? 0,
                    RecentPurchases = purchases.Data?.Take(10).ToList() ?? new(),
                    StockItems = stocks.Data ?? new(),
                    RecentTransactions = transactions.Data?.Take(10).ToList() ?? new()
                };

                return ApiResponse<InventoryDashboardViewModel>.SuccessResult(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory dashboard");
                return ApiResponse<InventoryDashboardViewModel>.FailureResult("Dashboard verileri getirilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<List<InventoryStockDto>>> GetLowStockItemsAsync()
        {
            try
            {
                var lowStockItems = await _context.InventoryStocks
                    .Where(s => s.IsActive && s.CurrentStock <= s.MinimumStock)
                    .OrderBy(s => s.ItemName)
                    .ToListAsync();

                var dtos = lowStockItems.Select(MapToInventoryStockDto).ToList();
                return ApiResponse<List<InventoryStockDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock items");
                return ApiResponse<List<InventoryStockDto>>.FailureResult("Düşük stok kalemleri getirilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalStockValueAsync()
        {
            try
            {
                var totalValue = await _context.InventoryStocks
                    .Where(s => s.IsActive)
                    .SumAsync(s => s.StockValue);

                return ApiResponse<decimal>.SuccessResult(totalValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total stock value");
                return ApiResponse<decimal>.FailureResult("Toplam stok değeri hesaplanırken hata oluştu");
            }
        }

        #endregion

        #region Yardımcı Metodlar

        public async Task<ApiResponse<bool>> UpdateAverageCostAsync(string itemName)
        {
            try
            {
                var stock = await _context.InventoryStocks
                    .FirstOrDefaultAsync(s => s.ItemName == itemName && s.IsActive);

                if (stock == null)
                    return ApiResponse<bool>.FailureResult("Stok kalemi bulunamadı");

                var purchases = await _context.Inventories
                    .Where(i => i.ItemName == itemName && i.IsActive)
                    .ToListAsync();

                if (purchases.Any())
                {
                    var totalCost = purchases.Sum(p => p.TotalAmount);
                    var totalQuantity = purchases.Sum(p => p.Quantity);
                    stock.AverageCost = totalQuantity > 0 ? totalCost / totalQuantity : 0;
                }

                stock.LastUpdated = DateTimeHelper.NowTurkey;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Ortalama maliyet güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating average cost: {ItemName}", itemName);
                return ApiResponse<bool>.FailureResult("Ortalama maliyet güncellenirken hata oluştu");
            }
        }

        public async Task<ApiResponse<List<string>>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _context.Inventories
                    .Where(i => i.IsActive && !string.IsNullOrEmpty(i.Category))
                    .Select(i => i.Category!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return ApiResponse<List<string>>.SuccessResult(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return ApiResponse<List<string>>.FailureResult("Kategoriler getirilirken hata oluştu");
            }
        }

        public async Task<ApiResponse<List<string>>> GetSuppliersAsync()
        {
            try
            {
                var suppliers = await _context.Inventories
                    .Where(i => i.IsActive && !string.IsNullOrEmpty(i.Supplier))
                    .Select(i => i.Supplier!)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToListAsync();

                return ApiResponse<List<string>>.SuccessResult(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suppliers");
                return ApiResponse<List<string>>.FailureResult("Tedarikçiler getirilirken hata oluştu");
            }
        }

        #endregion



        #region Private Helper Methods

        private async Task UpdateStockFromPurchaseAsync(Inventory inventory)
        {
            var stock = await _context.InventoryStocks
                .FirstOrDefaultAsync(s => s.ItemName == inventory.ItemName && s.IsActive);

            if (stock == null)
            {
                // Yeni stok kalemi oluştur
                stock = new InventoryStock
                {
                    ItemName = inventory.ItemName,
                    Category = inventory.Category,
                    CurrentStock = inventory.Quantity,
                    Unit = inventory.Unit,
                    AverageCost = inventory.UnitPrice
                };
                _context.InventoryStocks.Add(stock);
            }
            else
            {
                // Mevcut stoku güncelle
                var totalStock = stock.CurrentStock + inventory.Quantity;
                var totalValue = (stock.CurrentStock * stock.AverageCost) + inventory.TotalAmount;
                stock.CurrentStock = totalStock;
                stock.AverageCost = totalStock > 0 ? totalValue / totalStock : inventory.UnitPrice;
                stock.LastUpdated = DateTimeHelper.NowTurkey;
            }

            await _context.SaveChangesAsync();
        }

        private static InventoryDto MapToInventoryDto(Inventory inventory)
        {
            return new InventoryDto
            {
                Id = inventory.Id,
                ItemName = inventory.ItemName,
                Category = inventory.Category,
                UnitPrice = inventory.UnitPrice,
                Quantity = inventory.Quantity,
                Unit = inventory.Unit,
                TotalAmount = inventory.TotalAmount,
                Supplier = inventory.Supplier,
                PurchaseDate = inventory.PurchaseDate,
                Notes = inventory.Notes,
                CreatedAt = inventory.CreatedAt
            };
        }

        private static InventoryStockDto MapToInventoryStockDto(InventoryStock stock)
        {
            return new InventoryStockDto
            {
                Id = stock.Id,
                ItemName = stock.ItemName,
                Category = stock.Category,
                CurrentStock = stock.CurrentStock,
                MinimumStock = stock.MinimumStock,
                Unit = stock.Unit,
                AverageCost = stock.AverageCost,
                StockValue = stock.StockValue,
                LastUpdated = stock.LastUpdated
            };
        }

        private static InventoryTransactionDto MapToInventoryTransactionDto(InventoryTransaction transaction)
        {
            return new InventoryTransactionDto
            {
                Id = transaction.Id,
                ItemName = transaction.ItemName,
                TransactionType = transaction.TransactionType,
                Quantity = transaction.Quantity,
                Unit = transaction.Unit,
                UnitPrice = transaction.UnitPrice,
                TotalAmount = transaction.TotalAmount,
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate
            };
        }

        #endregion
    }
}
