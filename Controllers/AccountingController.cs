using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class AccountingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AccountingController(ApplicationDbContext context, IAuthService authService, IConfiguration configuration)
        {
            _context = context;
            _authService = authService;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        }

        private async Task<bool> IsAdminAsync()
        {
            return await _authService.IsCurrentUserAdminAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (!await IsAdminAsync())
            {
                return RedirectToAction("Login", "Admin");
            }

            var now = DateTime.Now;
            var start = startDate ?? new DateTime(now.Year, now.Month, 1);
            var end = endDate ?? start.AddMonths(1).AddDays(-1);

            // Ciro (teslim edilen siparişler)
            decimal revenue = await _context.Orders
                .Where(o => o.OrderStatus == ApplicationConstants.OrderStatus.Delivered && o.OrderDate >= start && o.OrderDate <= end)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            decimal purchasesTotal = 0m;
            decimal expensesTotal = 0m;
            var purchases = new List<PurchaseDto>();
            var expenses = new List<ExpenseDto>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Toplam alış maliyetleri (Quantity * UnitPrice)
                using (var cmd = new SqlCommand(@"SELECT ISNULL(SUM(CAST(Quantity as decimal(18,3)) * CAST(UnitPrice as decimal(18,2))), 0)
                                                  FROM Purchases WITH (NOLOCK)
                                                  WHERE [Date] BETWEEN @s AND @e", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start);
                    cmd.Parameters.AddWithValue("@e", end);
                    purchasesTotal = (decimal)await cmd.ExecuteScalarAsync();
                }

                // Toplam giderler
                using (var cmd = new SqlCommand(@"SELECT ISNULL(SUM(CAST(Amount as decimal(18,2))), 0)
                                                  FROM Expenses WITH (NOLOCK)
                                                  WHERE [Date] BETWEEN @s AND @e", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start);
                    cmd.Parameters.AddWithValue("@e", end);
                    expensesTotal = (decimal)await cmd.ExecuteScalarAsync();
                }

                // Son kayıtlar (liste)
                using (var cmd = new SqlCommand(@"SELECT TOP 100 Id, [Date], Supplier, Item, Quantity, Unit, UnitPrice, Notes
                                                  FROM Purchases WITH (NOLOCK)
                                                  WHERE [Date] BETWEEN @s AND @e
                                                  ORDER BY [Date] DESC, Id DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start);
                    cmd.Parameters.AddWithValue("@e", end);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        purchases.Add(new PurchaseDto
                        {
                            Id = reader.GetInt32(0),
                            Date = reader.GetDateTime(1),
                            Supplier = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Item = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Quantity = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader[4]),
                            Unit = reader.IsDBNull(5) ? null : reader.GetString(5),
                            UnitPrice = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader[6]),
                            Notes = reader.IsDBNull(7) ? null : reader.GetString(7)
                        });
                    }
                }

                using (var cmd = new SqlCommand(@"SELECT TOP 100 Id, [Date], Category, Description, Amount, PaymentMethod
                                                  FROM Expenses WITH (NOLOCK)
                                                  WHERE [Date] BETWEEN @s AND @e
                                                  ORDER BY [Date] DESC, Id DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@s", start);
                    cmd.Parameters.AddWithValue("@e", end);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        expenses.Add(new ExpenseDto
                        {
                            Id = reader.GetInt32(0),
                            Date = reader.GetDateTime(1),
                            Category = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Amount = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader[4]),
                            PaymentMethod = reader.IsDBNull(5) ? null : reader.GetString(5)
                        });
                    }
                }
            }
            catch (SqlException)
            {
                // Tablolar yoksa uyarı gösterelim; sayfa yine de açılır
                TempData["Info"] = "Muhasebe tabloları bulunamadı. Aşağıdaki örnek SQL ile 'Purchases' ve 'Expenses' tablolarını oluşturabilirsiniz.";
            }

            var vm = new AccountingDashboardViewModel
            {
                StartDate = start,
                EndDate = end,
                Revenue = revenue,
                PurchasesTotal = purchasesTotal,
                ExpensesTotal = expensesTotal,
                NetProfit = revenue - (purchasesTotal + expensesTotal),
                Purchases = purchases,
                Expenses = expenses
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPurchase(PurchaseInput input)
        {
            if (!await IsAdminAsync())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (input.Date == default)
            {
                input.Date = DateTime.Now;
            }

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"INSERT INTO Purchases ([Date], Supplier, Item, Quantity, Unit, UnitPrice, Notes)
                                           VALUES (@Date, @Supplier, @Item, @Quantity, @Unit, @UnitPrice, @Notes)", conn);
                cmd.Parameters.AddWithValue("@Date", input.Date);
                cmd.Parameters.AddWithValue("@Supplier", (object?)input.Supplier ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Item", (object?)input.Item ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", input.Quantity);
                cmd.Parameters.AddWithValue("@Unit", (object?)input.Unit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UnitPrice", input.UnitPrice);
                cmd.Parameters.AddWithValue("@Notes", (object?)input.Notes ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();

                TempData["Success"] = "Alış kaydı eklendi.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Alış kaydı eklenemedi. Lütfen tabloların mevcut olduğundan emin olun.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(ExpenseInput input)
        {
            if (!await IsAdminAsync())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (input.Date == default)
            {
                input.Date = DateTime.Now;
            }

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"INSERT INTO Expenses ([Date], Category, Description, Amount, PaymentMethod)
                                           VALUES (@Date, @Category, @Description, @Amount, @PaymentMethod)", conn);
                cmd.Parameters.AddWithValue("@Date", input.Date);
                cmd.Parameters.AddWithValue("@Category", (object?)input.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object?)input.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Amount", input.Amount);
                cmd.Parameters.AddWithValue("@PaymentMethod", (object?)input.PaymentMethod ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();

                TempData["Success"] = "Gider kaydı eklendi.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Gider kaydı eklenemedi. Lütfen tabloların mevcut olduğundan emin olun.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

