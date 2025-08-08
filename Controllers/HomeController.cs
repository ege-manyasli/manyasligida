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
        private readonly ISiteSettingsService _siteSettingsService;

        public HomeController(ApplicationDbContext context, ISiteSettingsService siteSettingsService)
        {
            _context = context;
            _siteSettingsService = siteSettingsService;
        }

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // Cache for 5 minutes
        public async Task<IActionResult> Index()
        {
            try
            {
                // Optimize database queries by running them in parallel
                var popularProductsTask = _context.Products
                    .Where(p => p.IsActive && p.IsPopular)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToListAsync();

                var recentBlogsTask = _context.Blogs
                    .Where(b => b.IsActive)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                var categoriesTask = _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .Take(6)
                    .ToListAsync();

                // Execute all queries in parallel
                await Task.WhenAll(popularProductsTask, recentBlogsTask, categoriesTask);

                var popularProducts = await popularProductsTask;
                var recentBlogs = await recentBlogsTask;
                var categories = await categoriesTask;

                // Eğer popüler ürün yoksa, son eklenen ürünleri getir
                if (!popularProducts.Any())
                {
                    popularProducts = await _context.Products
                        .Where(p => p.IsActive)
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(8)
                        .ToListAsync();
                }

                // Cache statistics for better performance
                var statsTask = Task.Run(async () =>
                {
                    var totalProducts = await _context.Products.Where(p => p.IsActive).CountAsync();
                    var totalOrders = await _context.Orders.CountAsync();
                    var totalUsers = await _context.Users.Where(u => u.IsActive).CountAsync();
                    var totalBlogs = await _context.Blogs.Where(b => b.IsActive).CountAsync();

                    return new
                    {
                        TotalProducts = totalProducts,
                        TotalOrders = totalOrders,
                        TotalUsers = totalUsers,
                        TotalBlogs = totalBlogs
                    };
                });

                var siteSettings = _siteSettingsService.Get();
                var stats = await statsTask;

                ViewBag.PopularProducts = popularProducts;
                ViewBag.RecentBlogs = recentBlogs;
                ViewBag.Categories = categories;
                ViewBag.Stats = stats;
                ViewBag.SiteSettings = siteSettings;

                return View();
            }
            catch (Exception ex)
            {
                // Log the error
                ViewBag.ErrorMessage = "Veriler yüklenirken bir hata oluştu.";
                return View();
            }
        }

        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)] // Cache for 10 minutes
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)] // Cache for 10 minutes
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
