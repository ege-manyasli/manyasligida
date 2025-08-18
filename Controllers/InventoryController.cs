using Microsoft.AspNetCore.Mvc;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;
using manyasligida.Attributes;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    [AdminAuthorization]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        #region Dashboard

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var startDate = DateTimeHelper.TodayTurkey.AddMonths(-1);
                var endDate = DateTimeHelper.TodayTurkey;

                var dashboardResult = await _inventoryService.GetDashboardAsync(startDate, endDate);
                
                if (dashboardResult.Success)
                {
                    return View(dashboardResult.Data);
                }
                else
                {
                    TempData["Error"] = dashboardResult.Message;
                    return View(new InventoryDashboardViewModel
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory dashboard");
                TempData["Error"] = "Dashboard yüklenirken bir hata oluştu";
                return View(new InventoryDashboardViewModel
                {
                    StartDate = DateTimeHelper.TodayTurkey.AddMonths(-1),
                    EndDate = DateTimeHelper.TodayTurkey
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDashboardData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = await _inventoryService.GetDashboardAsync(startDate, endDate);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data");
                return Json(ApiResponse<InventoryDashboardViewModel>.FailureResult("Dashboard verileri alınamadı"));
            }
        }

        #endregion

        #region Mal Alımı İşlemleri

        [HttpGet("Purchases")]
        public async Task<IActionResult> Purchases()
        {
            try
            {
                var purchasesResult = await _inventoryService.GetPurchasesAsync();
                
                if (purchasesResult.Success)
                {
                    ViewBag.Purchases = purchasesResult.Data;
                }
                else
                {
                    ViewBag.Purchases = new List<InventoryDto>();
                    TempData["Error"] = purchasesResult.Message;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchases");
                TempData["Error"] = "Mal alımları yüklenirken bir hata oluştu";
                ViewBag.Purchases = new List<InventoryDto>();
                return View();
            }
        }

        [HttpPost("Purchases/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPurchase(InventoryInput input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Geçersiz veri", errors });
                }

                var result = await _inventoryService.AddPurchaseAsync(input);

                _logger.LogInformation("Purchase creation attempt: {Success}, Item: {ItemName}", 
                    result.Success, input.ItemName);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding purchase");
                return Json(new { success = false, message = "Mal alımı eklenirken bir hata oluştu" });
            }
        }

        [HttpPost("Purchases/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            try
            {
                var result = await _inventoryService.DeletePurchaseAsync(id);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase: {Id}", id);
                return Json(new { success = false, message = "Mal alımı silinirken bir hata oluştu" });
            }
        }

        #endregion

        #region Stok Takibi

        [HttpGet("Stock")]
        public async Task<IActionResult> Stock()
        {
            try
            {
                var stockResult = await _inventoryService.GetStockItemsAsync();
                
                if (stockResult.Success)
                {
                    ViewBag.StockItems = stockResult.Data;
                }
                else
                {
                    ViewBag.StockItems = new List<InventoryStockDto>();
                    TempData["Error"] = stockResult.Message;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stock items");
                TempData["Error"] = "Stok kalemleri yüklenirken bir hata oluştu";
                ViewBag.StockItems = new List<InventoryStockDto>();
                return View();
            }
        }

        [HttpPost("Stock/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStockItem(InventoryStockInput input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Geçersiz veri", errors });
                }

                var result = await _inventoryService.AddStockItemAsync(input);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock item");
                return Json(new { success = false, message = "Stok kalemi eklenirken bir hata oluştu" });
            }
        }

        [HttpPost("Stock/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStockItem(int id)
        {
            try
            {
                var result = await _inventoryService.DeleteStockItemAsync(id);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stock item: {Id}", id);
                return Json(new { success = false, message = "Stok kalemi silinirken bir hata oluştu" });
            }
        }

        #endregion

        #region Stok İşlemleri

        [HttpGet("Transactions")]
        public async Task<IActionResult> Transactions()
        {
            try
            {
                var transactionsResult = await _inventoryService.GetTransactionsAsync();
                
                if (transactionsResult.Success)
                {
                    ViewBag.Transactions = transactionsResult.Data;
                }
                else
                {
                    ViewBag.Transactions = new List<InventoryTransactionDto>();
                    TempData["Error"] = transactionsResult.Message;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transactions");
                TempData["Error"] = "İşlemler yüklenirken bir hata oluştu";
                ViewBag.Transactions = new List<InventoryTransactionDto>();
                return View();
            }
        }

        [HttpPost("Transactions/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTransaction(InventoryTransactionInput input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Geçersiz veri", errors });
                }

                var result = await _inventoryService.AddTransactionAsync(input);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding transaction");
                return Json(new { success = false, message = "İşlem eklenirken bir hata oluştu" });
            }
        }

        #endregion

        #region Raporlar

        [HttpGet("Reports/LowStock")]
        public async Task<IActionResult> LowStockReport()
        {
            try
            {
                var result = await _inventoryService.GetLowStockItemsAsync();
                
                if (result.Success)
                {
                    ViewBag.LowStockItems = result.Data;
                }
                else
                {
                    ViewBag.LowStockItems = new List<InventoryStockDto>();
                    TempData["Error"] = result.Message;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading low stock report");
                TempData["Error"] = "Düşük stok raporu yüklenirken bir hata oluştu";
                ViewBag.LowStockItems = new List<InventoryStockDto>();
                return View();
            }
        }

        [HttpGet("Reports/StockValue")]
        public async Task<IActionResult> StockValueReport()
        {
            try
            {
                var stockResult = await _inventoryService.GetStockItemsAsync();
                var totalValueResult = await _inventoryService.GetTotalStockValueAsync();
                
                ViewBag.StockItems = stockResult.Success ? stockResult.Data : new List<InventoryStockDto>();
                ViewBag.TotalStockValue = totalValueResult.Success ? totalValueResult.Data : 0;

                if (!stockResult.Success)
                    TempData["Error"] = stockResult.Message;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stock value report");
                TempData["Error"] = "Stok değeri raporu yüklenirken bir hata oluştu";
                ViewBag.StockItems = new List<InventoryStockDto>();
                ViewBag.TotalStockValue = 0;
                return View();
            }
        }

        #endregion

        #region API Endpoints

        [HttpGet("API/Categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var result = await _inventoryService.GetCategoriesAsync();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return Json(ApiResponse<List<string>>.FailureResult("Kategoriler alınamadı"));
            }
        }

        [HttpGet("API/Suppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                var result = await _inventoryService.GetSuppliersAsync();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suppliers");
                return Json(ApiResponse<List<string>>.FailureResult("Tedarikçiler alınamadı"));
            }
        }

        [HttpGet("API/Stock/{itemName}")]
        public async Task<IActionResult> GetStockItem(string itemName)
        {
            try
            {
                var result = await _inventoryService.GetStockItemByNameAsync(itemName);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock item: {ItemName}", itemName);
                return Json(ApiResponse<InventoryStockDto>.FailureResult("Stok kalemi alınamadı"));
            }
        }

        #endregion
    }
}
