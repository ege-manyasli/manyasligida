using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public FAQController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: FAQ/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var faqs = await _context.FAQs
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();

                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                return View(faqs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "SSS yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<FAQ>());
            }
        }
    }
} 