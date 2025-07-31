using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public CartController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Cart/Index
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            var cartItems = new List<CartItemViewModel>();

            foreach (var item in cart)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
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

            ViewBag.CartTotal = _cartService.GetCartTotal();
            ViewBag.CartItemCount = _cartService.GetCartItemCount();

            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == productId && p.IsActive);
                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }

                if (product.StockQuantity < quantity)
                {
                    return Json(new { success = false, message = "Yeterli stok bulunmuyor." });
                }

                _cartService.AddToCart(product, quantity);

                return Json(new { 
                    success = true, 
                    message = "Ürün sepete eklendi.", 
                    cartItemCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == productId && p.IsActive);
                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı." });
                }

                if (quantity > product.StockQuantity)
                {
                    return Json(new { success = false, message = "Yeterli stok bulunmuyor." });
                }

                _cartService.UpdateQuantity(productId, quantity);

                return Json(new { 
                    success = true, 
                    message = "Miktar güncellendi.", 
                    cartItemCount = _cartService.GetCartItemCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
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
    }

} 