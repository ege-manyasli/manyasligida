using System;
using System.Diagnostics;
using manyasligida.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISiteSettingsService _siteSettingsService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IServiceProvider serviceProvider, ISiteSettingsService siteSettingsService, ILogger<HomeController> logger)
        {
            _serviceProvider = serviceProvider;
            _siteSettingsService = siteSettingsService;
            _logger = logger;
        }

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // Cache for 5 minutes
        public async Task<IActionResult> Index()
        {
            // Clear cookie consent for testing (remove this line later)
            HttpContext.Session.Remove("CookieConsent");
            
            try
            {
                _logger.LogInformation("Home page requested");

                // Create separate DbContext instances to avoid threading issues
                using var productsContext = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
                using var categoriesContext = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Get products with separate context
                var products = await productsContext.Products
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.IsPopular)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(8)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Price,
                        p.ImageUrl,
                        p.IsPopular,
                        p.CreatedAt,
                        p.Weight,
                        p.FatContent,
                        p.OldPrice,
                        IsNew = p.CreatedAt > DateTime.Now.AddDays(-30)
                    })
                    .ToListAsync();

                // Get categories with separate context
                var categories = await categoriesContext.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description
                    })
                    .ToListAsync();

                // Get site settings
                var siteSettings = await Task.Run(() => _siteSettingsService.Get());

                // Get current user info from session
                var currentUser = new
                {
                    IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")),
                    UserName = HttpContext.Session.GetString("UserName"),
                    UserEmail = HttpContext.Session.GetString("UserEmail"),
                    IsAdmin = HttpContext.Session.GetString("IsAdmin") == "True"
                };

                var viewModel = new
                {
                    Products = products,
                    Categories = categories,
                    SiteSettings = siteSettings ?? new SiteSettings(),
                    CurrentUser = currentUser
                };

                _logger.LogInformation($"Home page loaded successfully. Products: {products.Count}, Categories: {categories.Count}");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                
                // Return empty data instead of throwing
                var emptyViewModel = new
                {
                    Products = new List<object>(),
                    Categories = new List<object>(),
                    SiteSettings = _siteSettingsService.Get() ?? new SiteSettings(),
                    CurrentUser = new
                    {
                        IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")),
                        UserName = HttpContext.Session.GetString("UserName"),
                        UserEmail = HttpContext.Session.GetString("UserEmail"),
                        IsAdmin = HttpContext.Session.GetString("IsAdmin") == "True"
                    }
                };

                ViewBag.ErrorMessage = "Ürünler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return View(emptyViewModel);
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
