using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    public class BlogController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CartService _cartService;
        private readonly ISiteSettingsService _siteSettingsService;

        public BlogController(IServiceProvider serviceProvider, CartService cartService, ISiteSettingsService siteSettingsService)
        {
            _serviceProvider = serviceProvider;
            _cartService = cartService;
            _siteSettingsService = siteSettingsService;
        }

        // GET: Blog/Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var query = context.Blogs.Where(b => b.IsActive);
                var totalBlogs = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

                var blogs = await query
                    .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new Blog
                    {
                        Id = b.Id,
                        Title = b.Title ?? "",
                        Content = b.Content ?? "",
                        ImageUrl = b.ImageUrl ?? "",
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt,
                        PublishedAt = b.PublishedAt,
                        Summary = b.Summary ?? "",
                        Author = b.Author ?? ""
                    })
                    .ToListAsync();

                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                
                var siteSettings = await _siteSettingsService.GetAsync();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPreviousPage = page > 1;
                ViewBag.HasNextPage = page < totalPages;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;

                return View(blogs);
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                var siteSettings = new
                {
                    Phone = "+90 266 123 45 67",
                    Email = "info@manyasligida.com",
                    Address = "Manyas, Balıkesir",
                    WorkingHours = "Pzt-Cmt: 08:00-18:00",
                    FacebookUrl = "#",
                    InstagramUrl = "#",
                    TwitterUrl = "#",
                    YoutubeUrl = "#"
                };
                
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                
                TempData["Error"] = "Blog yazıları yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Blog>());
            }
        }

        // GET: Blog/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var blog = await context.Blogs
                    .Where(b => b.Id == id && b.IsActive)
                    .Select(b => new Blog
                    {
                        Id = b.Id,
                        Title = b.Title ?? "",
                        Content = b.Content ?? "",
                        ImageUrl = b.ImageUrl ?? "",
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt,
                        PublishedAt = b.PublishedAt,
                        Summary = b.Summary ?? "",
                        Author = b.Author ?? ""
                    })
                    .FirstOrDefaultAsync();

                if (blog == null)
                {
                    return NotFound();
                }

                // Son blog yazıları
                var recentBlogs = await context.Blogs
                    .Where(b => b.IsActive && b.Id != id)
                    .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                    .Take(3)
                    .Select(b => new Blog
                    {
                        Id = b.Id,
                        Title = b.Title ?? "",
                        Content = b.Content ?? "",
                        ImageUrl = b.ImageUrl ?? "",
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt,
                        PublishedAt = b.PublishedAt,
                        Summary = b.Summary ?? "",
                        Author = b.Author ?? ""
                    })
                    .ToListAsync();

                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                
                // Site ayarları
                var siteSettings = new
                {
                    Phone = "+90 266 123 45 67",
                    Email = "info@manyasligida.com",
                    Address = "Manyas, Balıkesir",
                    WorkingHours = "Pzt-Cmt: 08:00-18:00",
                    FacebookUrl = "#",
                    InstagramUrl = "#",
                    TwitterUrl = "#",
                    YoutubeUrl = "#"
                };

                ViewBag.RecentBlogs = recentBlogs;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;

                return View(blog);
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                var siteSettings = new
                {
                    Phone = "+90 266 123 45 67",
                    Email = "info@manyasligida.com",
                    Address = "Manyas, Balıkesir",
                    WorkingHours = "Pzt-Cmt: 08:00-18:00",
                    FacebookUrl = "#",
                    InstagramUrl = "#",
                    TwitterUrl = "#",
                    YoutubeUrl = "#"
                };
                
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                
                TempData["Error"] = "Blog detayı yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 