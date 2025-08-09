using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using manyasligida.Data;
using manyasligida.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext context,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _context = context;
            _serviceProvider = serviceProvider;
        }

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // Cache for 5 minutes
        public async Task<IActionResult> Index()
        {
            // Clear cookie consent for testing (remove this line later)
            HttpContext.Session.Remove("CookieConsent");
            
            try
            {
                _logger.LogInformation("Home page requested");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Try to get home service - if fails, use fallback
                var homeService = scope.ServiceProvider.GetService<manyasligida.Services.Interfaces.IHomeService>();
                if (homeService == null)
                {
                    _logger.LogWarning("HomeService not found, using fallback");
                    ViewBag.HomeContent = null;
                }
                else
                {
                    // Get home content from database
                    var homeResult = await homeService.GetHomeContentAsync();
                    ViewBag.HomeContent = homeResult.Data;
                }

                // Get products
                var products = await context.Products
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.IsPopular)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(8)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.ImageUrl,
                        p.Price
                    })
                    .ToListAsync();

                // Get categories
                var categories = await context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        ProductCount = context.Products.Count(p => p.CategoryId == c.Id && p.IsActive)
                    })
                    .ToListAsync();

                // Get blog posts
                var blogs = await context.Blogs
                    .Where(b => b.IsActive)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(3)
                    .Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.Content,
                        b.ImageUrl,
                        b.CreatedAt
                    })
                    .Cast<object>()
                    .ToListAsync();

                // Pass data to view
                ViewBag.Products = products;
                ViewBag.Categories = categories;
                ViewBag.Blogs = blogs;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                
                ViewBag.Products = new List<object>();
                ViewBag.Categories = new List<object>();
                ViewBag.Blogs = new List<object>();
                ViewBag.HomeContent = null;
                ViewBag.ErrorMessage = "Ürünler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}