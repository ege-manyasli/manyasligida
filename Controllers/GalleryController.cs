using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public GalleryController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Gallery/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var galleries = await _context.Galleries
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.DisplayOrder)
                    .ToListAsync();

                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                return View(galleries);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Galeri yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Gallery>());
            }
        }
    }
} 