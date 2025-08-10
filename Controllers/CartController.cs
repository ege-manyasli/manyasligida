using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    public class CartController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CartService _cartService;

        public CartController(IServiceProvider serviceProvider, CartService cartService)
        {
            _serviceProvider = serviceProvider;
            _cartService = cartService;
        }

        // GET: Cart/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var cart = _cartService.GetCart();
                var cartItems = new List<CartItemViewModel>();

                if (cart.Any())
                {
                    var productIds = cart.Select(c => c.ProductId).ToList();
                    var products = await context.Products
                        .Where(p => productIds.Contains(p.Id) && p.IsActive)
                        .ToDictionaryAsync(p => p.Id, p => p);

                    foreach (var item in cart)
                    {
                        if (products.TryGetValue(item.ProductId, out var product))
                        {
                            cartItems.Add(new CartItemViewModel
                            {
                                ProductId = item.ProductId,
                                ProductName = product.Name,
                                ProductImage = product.ImageUrl,
                                UnitPrice = item.UnitPrice,
                                Quantity = item.Quantity,
                                TotalPrice = item.TotalPrice,
                                Weight = product.Weight
                            });
                        }
                    }
                }

                ViewBag.CartTotal = _cartService.GetCartTotal();
                ViewBag.CartItemCount = _cartService.GetCartItemCount();

                return View(cartItems);
            }
            catch (Exception)
            {
                TempData["Error"] = "Sepet yüklenirken bir hata oluştu.";
                return View(new List<CartItemViewModel>());
            }
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    return Json(new { success = false, message = "Geçersiz miktar." });
                }

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
                
                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }

                // Sepetteki mevcut miktarı kontrol et
                var currentCart = _cartService.GetCart();
                var existingItem = currentCart.FirstOrDefault(c => c.ProductId == productId);
                var currentQuantityInCart = existingItem?.Quantity ?? 0;
                var totalRequestedQuantity = currentQuantityInCart + quantity;

                if (product.StockQuantity < totalRequestedQuantity)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Yeterli stok bulunmuyor. Mevcut stok: {product.StockQuantity}, Sepetinizde: {currentQuantityInCart}" 
                    });
                }

                _cartService.AddToCart(product, quantity);

                return Json(new { 
                    success = true, 
                    message = "Ürün sepete eklendi.", 
                    cartItemCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Bir hata oluştu." });
            }
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            try
            {
                if (quantity < 0)
                {
                    return Json(new { success = false, message = "Geçersiz miktar." });
                }

                if (quantity == 0)
                {
                    _cartService.RemoveFromCart(productId);
                    return Json(new { 
                        success = true, 
                        message = "Ürün sepetten kaldırıldı.", 
                        cartItemCount = _cartService.GetCartItemCount(),
                        cartTotal = _cartService.GetCartTotal()
                    });
                }

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
                
                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }

                if (quantity > product.StockQuantity)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Yeterli stok bulunmuyor. Mevcut stok: {product.StockQuantity}" 
                    });
                }

                _cartService.UpdateQuantity(productId, quantity);

                return Json(new { 
                    success = true, 
                    message = "Miktar güncellendi.", 
                    cartItemCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Bir hata oluştu." });
            }
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            try
            {
                _cartService.RemoveFromCart(productId);

                return Json(new { 
                    success = true, 
                    message = "Ürün sepetten kaldırıldı.", 
                    cartItemCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // POST: Cart/ClearCart
        [HttpPost]
        public IActionResult ClearCart()
        {
            try
            {
                _cartService.ClearCart();

                return Json(new { 
                    success = true, 
                    message = "Sepet temizlendi.", 
                    cartItemCount = 0,
                    cartTotal = 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // GET: Cart/GetCartCount
        [HttpGet]
        public IActionResult GetCartCount()
        {
            try
            {
                return Json(new { 
                    cartItemCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }
            catch
            {
                return Json(new { cartItemCount = 0, cartTotal = 0 });
            }
        }

        // GET: Cart/GetCartForWhatsApp
        [HttpGet]
        public async Task<IActionResult> GetCartForWhatsApp()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var cart = _cartService.GetCart();
                var cartItems = new List<object>();

                if (cart.Any())
                {
                    var productIds = cart.Select(c => c.ProductId).ToList();
                    var products = await context.Products
                        .Where(p => productIds.Contains(p.Id) && p.IsActive)
                        .ToDictionaryAsync(p => p.Id, p => p);

                    foreach (var item in cart)
                    {
                        if (products.TryGetValue(item.ProductId, out var product))
                        {
                            cartItems.Add(new
                            {
                                productId = item.ProductId,
                                productName = product.Name,
                                quantity = item.Quantity,
                                unitPrice = item.UnitPrice,
                                totalPrice = item.TotalPrice,
                                weight = product.Weight
                            });
                        }
                    }
                }

                return Json(new { 
                    success = true, 
                    cartItems = cartItems,
                    cartTotal = _cartService.GetCartTotal(),
                    cartItemCount = _cartService.GetCartItemCount()
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Sepet verileri alınamadı: " + ex.Message,
                    cartItems = new List<object>(),
                    cartTotal = 0,
                    cartItemCount = 0
                });
            }
        }
    }
} 