using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly ISiteSettingsService _siteSettingsService;

        public ProductsController(ApplicationDbContext context, CartService cartService, ISiteSettingsService siteSettingsService)
        {
            _context = context;
            _cartService = cartService;
            _siteSettingsService = siteSettingsService;
        }

        // GET: Products/Index
        [ResponseCache(Duration = 180, Location = ResponseCacheLocation.Any)] // Cache for 3 minutes
        public async Task<IActionResult> Index(int? categoryId, string? searchTerm, string? sortBy, int page = 1, int pageSize = 12)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive);

                // Kategori filtresi
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                // Arama filtresi
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => p.Name.Contains(searchTerm) || 
                                           (p.Description != null && p.Description.Contains(searchTerm)) ||
                                           (p.Ingredients != null && p.Ingredients.Contains(searchTerm)));
                }

                // Sıralama
                query = sortBy switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "name_asc" => query.OrderBy(p => p.Name),
                    "name_desc" => query.OrderByDescending(p => p.Name),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    "popular" => query.OrderByDescending(p => p.IsPopular).ThenByDescending(p => p.CreatedAt),
                    "featured" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
                };

                // Optimize pagination with parallel execution
                var totalProductsTask = query.CountAsync();
                var productsTask = query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var categoriesTask = _context.Categories.Where(c => c.IsActive).ToListAsync();

                await Task.WhenAll(totalProductsTask, productsTask, categoriesTask);

                var totalProducts = await totalProductsTask;
                var products = await productsTask;
                var categories = await categoriesTask;

                var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                var siteSettings = _siteSettingsService.Get();

                ViewBag.Categories = categories;
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SortBy = sortBy;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalProducts = totalProducts;
                ViewBag.PageSize = pageSize;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.SiteSettings = siteSettings;

                return View(products);
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                var siteSettings = _siteSettingsService.Get();
                
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                
                TempData["Error"] = "Ürünler yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Product>());
            }
        }

        // GET: Products/Detail/{id}
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // Cache for 5 minutes
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (product == null)
                {
                    return NotFound();
                }

                // Optimize related queries with parallel execution
                var relatedProductsTask = _context.Products
                    .Where(p => p.CategoryId == product.CategoryId && 
                               p.Id != product.Id && 
                               p.IsActive)
                    .OrderByDescending(p => p.IsPopular)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToListAsync();

                var popularProductsTask = _context.Products
                    .Where(p => p.IsPopular && p.IsActive && p.Id != product.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToListAsync();

                var categoriesTask = _context.Categories.Where(c => c.IsActive).ToListAsync();

                await Task.WhenAll(relatedProductsTask, popularProductsTask, categoriesTask);

                var relatedProducts = await relatedProductsTask;
                var popularProducts = await popularProductsTask;
                var categories = await categoriesTask;
                
                var siteSettings = _siteSettingsService.Get();

                ViewBag.RelatedProducts = relatedProducts;
                ViewBag.PopularProducts = popularProducts;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;

                return View(product);
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                var siteSettings = _siteSettingsService.Get();
                
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                
                TempData["Error"] = "Ürün detayları yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Products/Category/{id}
        [ResponseCache(Duration = 180, Location = ResponseCacheLocation.Any)] // Cache for 3 minutes
        public async Task<IActionResult> Category(int id, string? sortBy, int page = 1, int pageSize = 12)
        {
            try
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
                if (category == null)
                {
                    return NotFound();
                }

                var query = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == id && p.IsActive);

                // Sıralama
                query = sortBy switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "name_asc" => query.OrderBy(p => p.Name),
                    "name_desc" => query.OrderByDescending(p => p.Name),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    "popular" => query.OrderByDescending(p => p.IsPopular).ThenByDescending(p => p.CreatedAt),
                    "featured" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
                };

                // Optimize pagination
                var totalProductsTask = query.CountAsync();
                var productsTask = query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var categoriesTask = _context.Categories.Where(c => c.IsActive).ToListAsync();

                await Task.WhenAll(totalProductsTask, productsTask, categoriesTask);

                var totalProducts = await totalProductsTask;
                var products = await productsTask;
                var categories = await categoriesTask;

                var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                var siteSettings = _siteSettingsService.Get();

                ViewBag.Category = category;
                ViewBag.SortBy = sortBy;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalProducts = totalProducts;
                ViewBag.PageSize = pageSize;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;

                return View(products);
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
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
                
                TempData["Error"] = "Kategori ürünleri yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Product>());
            }
        }

        // GET: Products/Search
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)] // Cache for 1 minute
        public async Task<IActionResult> Search(string? q, string? sortBy, int page = 1, int pageSize = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return RedirectToAction("Index");
                }

                var query = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && 
                               (p.Name.Contains(q) || 
                                (p.Description != null && p.Description.Contains(q)) ||
                                (p.Ingredients != null && p.Ingredients.Contains(q))));

                // Sıralama
                query = sortBy switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "name_asc" => query.OrderBy(p => p.Name),
                    "name_desc" => query.OrderByDescending(p => p.Name),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    "popular" => query.OrderByDescending(p => p.IsPopular).ThenByDescending(p => p.CreatedAt),
                    "featured" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
                };

                // Optimize pagination
                var totalProductsTask = query.CountAsync();
                var productsTask = query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var categoriesTask = _context.Categories.Where(c => c.IsActive).ToListAsync();

                await Task.WhenAll(totalProductsTask, productsTask, categoriesTask);

                var totalProducts = await totalProductsTask;
                var products = await productsTask;
                var categories = await categoriesTask;

                var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                var siteSettings = _siteSettingsService.Get();

                ViewBag.SearchTerm = q;
                ViewBag.SortBy = sortBy;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalProducts = totalProducts;
                ViewBag.PageSize = pageSize;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;

                return View(products);
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
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
                
                TempData["Error"] = "Arama yapılırken bir hata oluştu: " + ex.Message;
                return View(new List<Product>());
            }
        }

        // GET: Products/Featured
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // Cache for 5 minutes
        public async Task<IActionResult> Featured()
        {
            try
            {
                var featuredProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsFeatured && p.IsActive)
                    .OrderBy(p => p.SortOrder)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                var siteSettings = _siteSettingsService.Get();

                ViewBag.FeaturedProducts = featuredProducts;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;

                return View(featuredProducts);
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
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
                
                TempData["Error"] = "Öne çıkan ürünler yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Product>());
            }
        }

        // GET: Products/New
        public async Task<IActionResult> New()
        {
            try
            {
                var newProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsNew && p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                
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

                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                
                return View(newProducts);
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
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
                
                TempData["Error"] = "Yeni ürünler yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Product>());
            }
        }

        // GET: Products/OnSale
        public async Task<IActionResult> OnSale()
        {
            try
            {
                var onSaleProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && p.OldPrice.HasValue && p.OldPrice > p.Price)
                    .OrderByDescending(p => p.DiscountPercentage)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                
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

                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                
                return View(onSaleProducts);
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
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
                
                TempData["Error"] = "İndirimli ürünler yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Product>());
            }
        }

        // GET: Products/QuickView/5
        public async Task<IActionResult> QuickView(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (product == null)
                {
                    return NotFound();
                }

                return PartialView("_QuickView", product);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ürün yüklenirken bir hata oluştu: " + ex.Message });
            }
        }

        // GET: Products/GetProductImages/5
        public async Task<IActionResult> GetProductImages(int id)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (product == null)
                {
                    return NotFound();
                }

                var imageUrls = product.GetImageUrls();
                return Json(new { success = true, images = imageUrls });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Görseller yüklenirken bir hata oluştu: " + ex.Message });
            }
        }

        // POST: Products/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

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
                    message = $"{product.Name} sepete eklendi.", 
                    cartItemCount = _cartService.GetCartItemCount() 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sepete eklenirken bir hata oluştu: " + ex.Message });
            }
        }

        // GET: Products/GetCategories
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                return Json(new { success = true, categories = categories });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kategoriler yüklenirken bir hata oluştu: " + ex.Message });
            }
        }
    }
} 