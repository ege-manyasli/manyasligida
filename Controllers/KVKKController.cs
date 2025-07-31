using Microsoft.AspNetCore.Mvc;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class KVKKController : Controller
    {
        private readonly CartService _cartService;

        public KVKKController(CartService cartService)
        {
            _cartService = cartService;
        }

        // GET: KVKK/Index
        public IActionResult Index()
        {
            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View();
        }

        // GET: KVKK/Gizlilik
        public IActionResult Gizlilik()
        {
            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View();
        }
    }
} 