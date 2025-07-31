using Microsoft.AspNetCore.Mvc;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class AboutController : Controller
    {
        private readonly CartService _cartService;

        public AboutController(CartService cartService)
        {
            _cartService = cartService;
        }

        // GET: About/Index
        public IActionResult Index()
        {
            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View();
        }
    }
} 