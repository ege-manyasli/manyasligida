using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using System.Security.Cryptography;
using System.Text;

namespace manyasligida.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public OrderController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Order/Index
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == int.Parse(userId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View(orders);
        }

        // GET: Order/Checkout
        public IActionResult Checkout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartService.GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Sepetiniz boş.";
                return RedirectToAction("Index", "Cart");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Cart = cart;
            ViewBag.CartTotal = _cartService.GetCartTotal();
            ViewBag.User = user;
            ViewBag.CartItemCount = _cartService.GetCartItemCount();

            return View();
        }

        // POST: Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(OrderViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartService.GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Sepetiniz boş.";
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var order = new Order
                    {
                        OrderNumber = GenerateOrderNumber(),
                        UserId = int.Parse(userId),
                        CustomerName = model.CustomerName,
                        CustomerEmail = model.CustomerEmail,
                        CustomerPhone = model.CustomerPhone,
                        ShippingAddress = model.ShippingAddress,
                        City = model.City,
                        PostalCode = model.PostalCode,
                        SubTotal = _cartService.GetCartTotal(),
                        ShippingCost = 0, // Ücretsiz kargo
                        TaxAmount = 0, // KDV dahil
                        TotalAmount = _cartService.GetCartTotal(),
                        OrderStatus = "Pending",
                        PaymentStatus = "Pending",
                        Notes = model.Notes,
                        OrderDate = DateTime.Now
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Order items ekle
                    foreach (var cartItem in cart)
                    {
                        var product = await _context.Products.FindAsync(cartItem.ProductId);
                        if (product != null)
                        {
                            var orderItem = new OrderItem
                            {
                                OrderId = order.Id,
                                ProductId = cartItem.ProductId,
                                ProductName = product.Name,
                                Quantity = cartItem.Quantity,
                                UnitPrice = cartItem.UnitPrice,
                                TotalPrice = cartItem.TotalPrice
                            };

                            _context.OrderItems.Add(orderItem);

                            // Stok güncelle
                            product.StockQuantity -= cartItem.Quantity;
                        }
                    }

                    await _context.SaveChangesAsync();

                    // Sepeti temizle
                    _cartService.ClearCart();

                    TempData["OrderSuccess"] = $"Siparişiniz başarıyla oluşturuldu. Sipariş numaranız: {order.OrderNumber}";
                    return RedirectToAction(nameof(OrderConfirmation), new { id = order.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Sipariş oluşturulurken bir hata oluştu: " + ex.Message);
                }
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            ViewBag.Cart = cart;
            ViewBag.CartTotal = _cartService.GetCartTotal();
            ViewBag.User = user;
            ViewBag.CartItemCount = _cartService.GetCartItemCount();

            return View("Checkout", model);
        }

        // GET: Order/OrderConfirmation/5
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == int.Parse(userId));

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View(order);
        }

        // GET: Order/OrderDetail/5
        public async Task<IActionResult> OrderDetail(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == int.Parse(userId));

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View(order);
        }

        private string GenerateOrderNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random();
            var randomPart = random.Next(1000, 9999).ToString();
            return $"ORD{timestamp}{randomPart}";
        }
    }
} 