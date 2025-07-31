using System;
using System.Diagnostics;
using manyasligida.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Services;
using System.Linq;
using System.Threading.Tasks;

namespace manyasligida.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Popüler ürünleri getir (IsPopular = true olanlar)
                var popularProducts = await _context.Products
                    .Where(p => p.IsActive && p.IsPopular)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToListAsync();

                // Son blog yazılarını getir
                var recentBlogs = await _context.Blogs
                    .Where(b => b.IsActive)
                    .OrderByDescending(b => b.PublishedAt)
                    .Take(3)
                    .ToListAsync();

                // Kategorileri getir
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .Take(6)
                    .ToListAsync();

                ViewBag.PopularProducts = popularProducts;
                ViewBag.RecentBlogs = recentBlogs;
                ViewBag.Categories = categories;

                return View();
            }
            catch
            {
                // Log the error
                ViewBag.ErrorMessage = "Veriler yüklenirken bir hata oluştu.";
                return View();
            }
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}
