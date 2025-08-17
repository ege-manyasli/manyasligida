using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Linq;
using System.Threading.Tasks;
using manyasligida.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    public class AdminController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CartService _cartService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IAuthService _authService;
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ISiteSettingsService _siteSettingsService;

        public AdminController(IServiceProvider serviceProvider, CartService cartService, IFileUploadService fileUploadService, IAuthService authService, ILogger<AdminController> logger, ApplicationDbContext context, ISiteSettingsService siteSettingsService)
        {
            _serviceProvider = serviceProvider;
            _cartService = cartService;
            _fileUploadService = fileUploadService;
            _authService = authService;
            _logger = logger;
            _context = context;
            _siteSettingsService = siteSettingsService;
        }

        // Admin kontrolü
        private async Task<bool> IsAdminAsync()
        {
            try
            {
                // Önce session'dan kontrol et
                var session = HttpContext.Session;
                var isAdmin = session.GetString(ApplicationConstants.SessionKeys.IsAdmin);
                var userId = session.GetString(ApplicationConstants.SessionKeys.UserId);

                if (!string.IsNullOrEmpty(userId) && isAdmin == "True")
                {
                    return true;
                }

                // Session'da yoksa AuthService'den kontrol et
                return await _authService.IsCurrentUserAdminAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking admin status");
                return false;
            }
        }

        // Sync admin check for backward compatibility
        private bool IsAdmin()
        {
            try
            {
                var session = HttpContext.Session;
                var isAdmin = session.GetString(ApplicationConstants.SessionKeys.IsAdmin);
                var userId = session.GetString(ApplicationConstants.SessionKeys.UserId);

                // User ID kontrolü ekle
                if (string.IsNullOrEmpty(userId))
                    return false;

                return isAdmin == "True";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking admin status");
                return false;
            }
        }

        // GET: Admin/Login
        public async Task<IActionResult> Login()
        {
            // Eğer zaten admin girişi yapılmışsa dashboard'a yönlendir
            if (await IsAdminAsync())
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _logger.LogInformation("Admin login attempt for email: {Email}", model.Email);
                
                // Doğrudan veritabanından kullanıcıyı al (null kontrolü ile)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email != null && 
                                             u.Email.ToLower() == model.Email.ToLower() && 
                                             u.IsActive);
                
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", model.Email);
                    ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                    return View(model);
                }
                
                // Basit şifre kontrolü
                if (user.Password != model.Password)
                {
                    _logger.LogWarning("Password mismatch for user: {UserId}", user.Id);
                    ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                    return View(model);
                }
                
                _logger.LogInformation("Login successful for user: {UserId}, IsAdmin: {IsAdmin}", user.Id, user.IsAdmin);
                
                if (user.IsAdmin)
                {
                                    // Session'a admin bilgilerini set et
                var session = HttpContext.Session;
                session.SetString(ApplicationConstants.SessionKeys.IsAdmin, "True");
                session.SetString(ApplicationConstants.SessionKeys.UserId, user.Id.ToString());
                session.SetString(ApplicationConstants.SessionKeys.UserName, user.FullName ?? $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim());
                session.SetString(ApplicationConstants.SessionKeys.UserEmail, user.Email ?? "");

                    TempData["LoginSuccess"] = "Admin paneline başarıyla giriş yaptınız!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Admin yetkisi bulunmayan kullanıcı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login");
                ModelState.AddModelError("", "Giriş sırasında bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View(model);
        }

        // GET: Admin/Index (Ana admin sayfası)
        public async Task<IActionResult> Index()
        {
            try
            {
                // Geçici olarak admin kontrolünü kaldır
                // if (!await IsAdminAsync())
                // {
                //     return RedirectToAction("Login", "Account");
                // }

                // Dashboard istatistikleri
                var totalProducts = await _context.Products.CountAsync();
                var totalCategories = await _context.Categories.CountAsync();
                var totalUsers = await _context.Users.CountAsync();
                var totalOrders = await _context.Orders.CountAsync();
                var totalBlogs = await _context.Blogs.CountAsync();
                var totalGalleries = await _context.Galleries.CountAsync();
                var totalVideos = await _context.Videos.CountAsync();
                var unreadMessages = await _context.ContactMessages.Where(m => !m.IsRead).CountAsync();
                var totalRevenue = await _context.Orders
                    .Where(o => o.OrderStatus == ApplicationConstants.OrderStatus.Delivered)
                    .SumAsync(o => o.TotalAmount);

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalBlogs = totalBlogs;
            ViewBag.TotalGalleries = totalGalleries;
            ViewBag.TotalVideos = totalVideos;
            ViewBag.UnreadMessages = unreadMessages;
            ViewBag.TotalRevenue = totalRevenue;

            // Son siparişler
                            var recentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new
                    {
                        o.Id,
                        OrderNumber = o.Id,
                        CustomerName = $"{o.User.FirstName ?? ""} {o.User.LastName ?? ""}".Trim(),
                        CustomerEmail = o.User.Email ?? "",
                        TotalAmount = o.TotalAmount,
                        o.OrderStatus,
                        o.OrderDate
                    })
                    .ToListAsync();

            // Son mesajlar
            var recentMessages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Son blog yazıları (sadece mevcut kolonları seç)
            var recentBlogs = await _context.Blogs
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .Select(b => new
                {
                    b.Id,
                    Title = b.Title ?? "",
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

                ViewBag.RecentOrders = recentOrders;
                ViewBag.RecentMessages = recentMessages;
                ViewBag.RecentBlogs = recentBlogs;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["Error"] = "Dashboard yüklenirken bir hata oluştu: " + ex.Message;
                return View();
            }
        }



        // GET: Admin/Products
        public async Task<IActionResult> Products()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Null değerleri güvenli hale getir
                foreach (var product in products)
                {
                    if (product.Category != null)
                    {
                        product.Category.Name = product.Category.Name ?? "";
                        product.Category.Description = product.Category.Description ?? "";
                    }
                }

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["Error"] = "Ürünler yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Product>());
            }
        }

        // GET: Admin/AddProduct
        public async Task<IActionResult> AddProduct()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

                // Eğer hiç kategori yoksa, varsayılan kategoriler oluştur
                if (!categories.Any())
                {
                    var defaultCategories = new List<Category>
                    {
                        new Category { Name = "Beyaz Peynir", Description = "Beyaz peynir çeşitleri", IsActive = true, DisplayOrder = 1 },
                        new Category { Name = "Kaşar Peyniri", Description = "Kaşar peyniri çeşitleri", IsActive = true, DisplayOrder = 2 },
                        new Category { Name = "Özel Peynirler", Description = "Özel peynir çeşitleri", IsActive = true, DisplayOrder = 3 }
                    };

                    _context.Categories.AddRange(defaultCategories);
                    await _context.SaveChangesAsync();

                    categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                }

                ViewBag.Categories = categories;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Sayfa yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Products");
            }
        }

        // POST: Admin/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFile? imageFile, List<IFormFile>? galleryFiles)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Resim yükleme işlemi (ana + galeri)
                    var imageUrls = new List<string>();
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(imageFile))
                        {
                            ModelState.AddModelError("ImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            var activeCategories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                            ViewBag.Categories = activeCategories;
                            return View(product);
                        }

                        var mainUrl = await _fileUploadService.UploadImageAsync(imageFile, "products");
                        product.ImageUrl = mainUrl;
                        imageUrls.Add(mainUrl);
                    }

                    if (galleryFiles != null && galleryFiles.Count > 0)
                    {
                        foreach (var gf in galleryFiles)
                        {
                            if (gf != null && gf.Length > 0 && _fileUploadService.IsValidImage(gf))
                            {
                                var url = await _fileUploadService.UploadImageAsync(gf, "products");
                                if (!imageUrls.Contains(url)) imageUrls.Add(url);
                            }
                        }
                    }

                    if (imageUrls.Count > 0)
                    {
                        product.SetImageUrls(imageUrls);
                        if (string.IsNullOrEmpty(product.ImageUrl))
                        {
                            product.ImageUrl = imageUrls.First();
                        }
                    }

                    // Ürün özelliklerini ayarla
                    product.CreatedAt = DateTimeHelper.NowTurkey;
                    product.IsActive = true;
                    product.UpdatedAt = DateTimeHelper.NowTurkey;

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ürün başarıyla eklendi.";
                    return RedirectToAction(nameof(Products));
                }
                else
                {
                    var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ürün eklenirken bir hata oluştu: " + ex.Message);
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
        }

        // GET: Admin/EditProduct/5
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Ürün bulunamadı.";
                    return RedirectToAction(nameof(Products));
                }

                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                ViewBag.Categories = categories;

                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ürün yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Products));
            }
        }

        // POST: Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? imageFile, List<IFormFile>? galleryFiles)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (id != product.Id)
                {
                    TempData["Error"] = "Ürün ID'si uyuşmuyor.";
                    return RedirectToAction(nameof(Products));
                }

                if (ModelState.IsValid)
                {
                    // Mevcut ürünü getir
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        TempData["Error"] = "Ürün bulunamadı.";
                        return RedirectToAction(nameof(Products));
                    }

                    // Görselleri birleştir (ana + galeri)
                    var mergedImageUrls = existingProduct.GetImageUrls();

                    // Yeni ana resim yüklendiyse
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(imageFile))
                        {
                            ModelState.AddModelError("ImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            var activeCategories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                            ViewBag.Categories = activeCategories;
                            return View(product);
                        }

                        // Eski ana resmi sil (diskten) ve listeden çıkar
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            await _fileUploadService.DeleteImageAsync(existingProduct.ImageUrl);
                            mergedImageUrls = mergedImageUrls.Where(u => u != existingProduct.ImageUrl).ToList();
                        }

                        var newMainUrl = await _fileUploadService.UploadImageAsync(imageFile, "products");
                        product.ImageUrl = newMainUrl;
                        mergedImageUrls.Insert(0, newMainUrl);
                    }
                    else
                    {
                        // Resim değişmediyse mevcut ana resmi koru
                        product.ImageUrl = existingProduct.ImageUrl;
                    }

                    // Ek galeri resimleri
                    if (galleryFiles != null && galleryFiles.Count > 0)
                    {
                        foreach (var gf in galleryFiles)
                        {
                            if (gf != null && gf.Length > 0 && _fileUploadService.IsValidImage(gf))
                            {
                                var url = await _fileUploadService.UploadImageAsync(gf, "products");
                                if (!mergedImageUrls.Contains(url)) mergedImageUrls.Add(url);
                            }
                        }
                    }

                    if (mergedImageUrls.Count > 0)
                    {
                        product.SetImageUrls(mergedImageUrls);
                    }

                    // Ürün özelliklerini güncelle
                    product.UpdatedAt = DateTimeHelper.NowTurkey;
                    product.CreatedAt = existingProduct.CreatedAt; // Mevcut oluşturma tarihini koru

                    // Mevcut ürünü güncelle
                    _context.Entry(existingProduct).CurrentValues.SetValues(product);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction(nameof(Products));
                }
                else
                {
                    var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ürün güncellenirken bir hata oluştu: " + ex.Message);
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
        }

        // POST: Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    // Ürünün siparişlerde kullanılıp kullanılmadığını kontrol et
                    var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
                    if (hasOrders)
                    {
                        TempData["Error"] = "Bu ürün siparişlerde kullanıldığı için silinemez. Bunun yerine pasif yapabilirsiniz.";
                        return RedirectToAction(nameof(Products));
                    }

                    // Ürün resmini sil
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        try
                        {
                            await _fileUploadService.DeleteImageAsync(product.ImageUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Product image could not be deleted: {ImageUrl}", product.ImageUrl);
                        }
                    }

                    // Ürünü tamamen sil
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"'{product.Name}' ürünü başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Ürün bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                TempData["Error"] = "Ürün silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Products));
        }

        // POST: Admin/ToggleProductStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProductStatus(int productId, bool isActive)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    product.IsActive = isActive;
                    product.UpdatedAt = DateTimeHelper.NowTurkey;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"'{product.Name}' ürünü başarıyla {(isActive ? "aktif" : "pasif")} yapıldı.";
                }
                else
                {
                    TempData["Error"] = "Ürün bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product status for ID: {ProductId}", productId);
                TempData["Error"] = "Ürün durumu güncellenirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Products));
        }

        // GET: Admin/Blog
        public async Task<IActionResult> Blog()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Sadece mevcut kolonları seç
                var blogs = await _context.Blogs
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new
                    {
                        b.Id,
                        Title = b.Title ?? "",
                        Content = b.Content ?? "",
                        ImageUrl = b.ImageUrl ?? "",
                        b.IsActive,
                        b.CreatedAt,
                        b.UpdatedAt,
                        b.PublishedAt
                    })
                    .ToListAsync();

                // Anonim tipi Blog nesnesine dönüştür
                var blogList = blogs.Select(b => new Blog
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    ImageUrl = b.ImageUrl,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    PublishedAt = b.PublishedAt,
                    Summary = "", // Varsayılan değer
                    Author = "" // Varsayılan değer
                }).ToList();

                return View(blogList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading blogs");
                TempData["Error"] = "Blog yazıları yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Blog>());
            }
        }

        // GET: Admin/AddBlog
        public IActionResult AddBlog()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Admin/AddBlog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBlog(Blog blog, IFormFile? imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Resim yükleme işlemi
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(imageFile))
                        {
                            ModelState.AddModelError("ImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            return View(blog);
                        }

                        var imageUrl = await _fileUploadService.UploadImageAsync(imageFile, "blog");
                        blog.ImageUrl = imageUrl;
                    }

                    // Null değerleri güvenli hale getir
                    blog.Title = blog.Title ?? "";
                    blog.Content = blog.Content ?? "";
                    blog.Summary = blog.Summary ?? "";
                    blog.Author = blog.Author ?? "";
                    blog.ImageUrl = blog.ImageUrl ?? "";
                    
                    blog.CreatedAt = DateTimeHelper.NowTurkey;
                    blog.IsActive = true;

                    _context.Blogs.Add(blog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Blog yazısı başarıyla eklendi.";
                    return RedirectToAction(nameof(Blog));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding blog");
                    ModelState.AddModelError("", "Blog yazısı eklenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(blog);
        }

        // GET: Admin/EditBlog/5
        public async Task<IActionResult> EditBlog(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null)
                {
                    TempData["Error"] = "Blog yazısı bulunamadı.";
                    return RedirectToAction(nameof(Blog));
                }

                return View(blog);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Blog yazısı yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Blog));
            }
        }

        // POST: Admin/EditBlog/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(int id, Blog blog, IFormFile? imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (id != blog.Id)
                {
                    TempData["Error"] = "Blog ID'si uyuşmuyor.";
                    return RedirectToAction(nameof(Blog));
                }

                if (ModelState.IsValid)
                {
                    // Mevcut blog'u getir
                    var existingBlog = await _context.Blogs.FindAsync(id);
                    if (existingBlog == null)
                    {
                        TempData["Error"] = "Blog yazısı bulunamadı.";
                        return RedirectToAction(nameof(Blog));
                    }

                    // Null değerleri güvenli hale getir
                    blog.Title = blog.Title ?? "";
                    blog.Content = blog.Content ?? "";
                    blog.Summary = blog.Summary ?? "";
                    blog.Author = blog.Author ?? "";

                    // Yeni resim yüklendiyse
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(imageFile))
                        {
                            ModelState.AddModelError("ImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            return View(blog);
                        }

                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(existingBlog.ImageUrl))
                        {
                            await _fileUploadService.DeleteImageAsync(existingBlog.ImageUrl);
                        }

                        // Yeni resmi yükle
                        var imageUrl = await _fileUploadService.UploadImageAsync(imageFile, "blog");
                        blog.ImageUrl = imageUrl;
                    }
                    else
                    {
                        // Resim değişmediyse mevcut resmi koru
                        blog.ImageUrl = existingBlog.ImageUrl ?? "";
                    }

                    // Blog özelliklerini güncelle
                    blog.UpdatedAt = DateTimeHelper.NowTurkey;
                    blog.CreatedAt = existingBlog.CreatedAt; // Mevcut oluşturma tarihini koru

                    // Mevcut blog'u güncelle
                    _context.Entry(existingBlog).CurrentValues.SetValues(blog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Blog yazısı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Blog));
                }
                else
                {
                    return View(blog);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog with ID: {BlogId}", id);
                ModelState.AddModelError("", "Blog yazısı güncellenirken bir hata oluştu: " + ex.Message);
                return View(blog);
            }
        }

        // POST: Admin/DeleteBlog/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                try
                {
                    // Blog resmini sil
                    if (!string.IsNullOrEmpty(blog.ImageUrl))
                    {
                        await _fileUploadService.DeleteImageAsync(blog.ImageUrl);
                    }

                    // Blog yazısını tamamen sil
                    _context.Blogs.Remove(blog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Blog yazısı başarıyla silindi.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Blog yazısı silinirken bir hata oluştu: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Blog));
        }

        // GET: Admin/Gallery
        public async Task<IActionResult> Gallery()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var galleries = await _context.Galleries
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync();

                // Null değerleri güvenli hale getir
                foreach (var gallery in galleries)
                {
                    gallery.Title = gallery.Title ?? "";
                    gallery.Description = gallery.Description ?? "";
                    gallery.ImageUrl = gallery.ImageUrl ?? "";
                }

                return View(galleries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading galleries");
                TempData["Error"] = "Galeri öğeleri yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Gallery>());
            }
        }

        // GET: Admin/AddGallery
        public IActionResult AddGallery()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Admin/AddGallery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGallery(Gallery gallery, IFormFile? imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Resim yükleme işlemi
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(imageFile))
                        {
                            ModelState.AddModelError("ImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            return View(gallery);
                        }

                        var imageUrl = await _fileUploadService.UploadImageAsync(imageFile, "gallery");
                        gallery.ImageUrl = imageUrl;
                    }

                    gallery.CreatedAt = DateTimeHelper.NowTurkey;
                    gallery.IsActive = true;

                    _context.Galleries.Add(gallery);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Galeri öğesi başarıyla eklendi.";
                    return RedirectToAction(nameof(Gallery));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Galeri öğesi eklenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(gallery);
        }

        // GET: Admin/EditGallery/5
        public async Task<IActionResult> EditGallery(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var gallery = await _context.Galleries.FindAsync(id);
                if (gallery == null)
                {
                    TempData["Error"] = "Galeri öğesi bulunamadı.";
                    return RedirectToAction(nameof(Gallery));
                }

                return View(gallery);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Galeri öğesi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Gallery));
            }
        }

        // POST: Admin/EditGallery/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGallery(int id, Gallery gallery, IFormFile? imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (id != gallery.Id)
                {
                    TempData["Error"] = "Galeri ID'si uyuşmuyor.";
                    return RedirectToAction(nameof(Gallery));
                }

                if (ModelState.IsValid)
                {
                    // Mevcut galeri öğesini getir
                    var existingGallery = await _context.Galleries.FindAsync(id);
                    if (existingGallery == null)
                    {
                        TempData["Error"] = "Galeri öğesi bulunamadı.";
                        return RedirectToAction(nameof(Gallery));
                    }

                    // Yeni resim yüklendiyse
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(imageFile))
                        {
                            ModelState.AddModelError("ImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            return View(gallery);
                        }

                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(existingGallery.ImageUrl))
                        {
                            await _fileUploadService.DeleteImageAsync(existingGallery.ImageUrl);
                        }

                        // Yeni resmi yükle
                        var imageUrl = await _fileUploadService.UploadImageAsync(imageFile, "gallery");
                        gallery.ImageUrl = imageUrl;
                    }
                    else
                    {
                        // Resim değişmediyse mevcut resmi koru
                        gallery.ImageUrl = existingGallery.ImageUrl;
                    }

                    // Galeri özelliklerini güncelle
                    gallery.UpdatedAt = DateTimeHelper.NowTurkey;
                    gallery.CreatedAt = existingGallery.CreatedAt; // Mevcut oluşturma tarihini koru

                    // Mevcut galeri öğesini güncelle
                    _context.Entry(existingGallery).CurrentValues.SetValues(gallery);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Galeri öğesi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Gallery));
                }
                else
                {
                    return View(gallery);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Galeri öğesi güncellenirken bir hata oluştu: " + ex.Message);
                return View(gallery);
            }
        }

        // POST: Admin/DeleteGallery/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGallery(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var gallery = await _context.Galleries.FindAsync(id);
            if (gallery != null)
            {
                try
                {
                    // Galeri resmini sil
                    if (!string.IsNullOrEmpty(gallery.ImageUrl))
                    {
                        await _fileUploadService.DeleteImageAsync(gallery.ImageUrl);
                    }

                    // Galeri öğesini tamamen sil
                    _context.Galleries.Remove(gallery);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Galeri öğesi başarıyla silindi.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Galeri öğesi silinirken bir hata oluştu: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Gallery));
        }

        // GET: Admin/FAQ
        public async Task<IActionResult> FAQ()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var faqs = await _context.FAQs
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();

                // Null değerleri güvenli hale getir
                foreach (var faq in faqs)
                {
                    faq.Question = faq.Question ?? "";
                    faq.Answer = faq.Answer ?? "";
                }

                return View(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FAQs");
                TempData["Error"] = "SSS öğeleri yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<FAQ>());
            }
        }

        // GET: Admin/AddFAQ
        public IActionResult AddFAQ()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Admin/AddFAQ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFAQ(FAQ faq)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                faq.CreatedAt = DateTimeHelper.NowTurkey;
                faq.IsActive = true;

                _context.FAQs.Add(faq);
                await _context.SaveChangesAsync();

                TempData["Success"] = "SSS öğesi başarıyla eklendi.";
                return RedirectToAction(nameof(FAQ));
            }

            return View(faq);
        }

        // GET: Admin/EditFAQ/5
        public async Task<IActionResult> EditFAQ(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var faq = await _context.FAQs.FindAsync(id);
                if (faq == null)
                {
                    TempData["Error"] = "SSS öğesi bulunamadı.";
                    return RedirectToAction(nameof(FAQ));
                }

                return View(faq);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "SSS öğesi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(FAQ));
            }
        }

        // POST: Admin/EditFAQ/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFAQ(int id, FAQ faq)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (id != faq.Id)
                {
                    TempData["Error"] = "SSS ID'si uyuşmuyor.";
                    return RedirectToAction(nameof(FAQ));
                }

                if (ModelState.IsValid)
                {
                    var existingFAQ = await _context.FAQs.FindAsync(id);
                    if (existingFAQ == null)
                    {
                        TempData["Error"] = "SSS öğesi bulunamadı.";
                        return RedirectToAction(nameof(FAQ));
                    }

                    // FAQ özelliklerini güncelle
                    faq.UpdatedAt = DateTimeHelper.NowTurkey;
                    faq.CreatedAt = existingFAQ.CreatedAt; // Mevcut oluşturma tarihini koru

                    // Mevcut FAQ'yu güncelle
                    _context.Entry(existingFAQ).CurrentValues.SetValues(faq);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "SSS öğesi başarıyla güncellendi.";
                    return RedirectToAction(nameof(FAQ));
                }
                else
                {
                    return View(faq);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "SSS öğesi güncellenirken bir hata oluştu: " + ex.Message);
                return View(faq);
            }
        }

        // POST: Admin/DeleteFAQ/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFAQ(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var faq = await _context.FAQs.FindAsync(id);
            if (faq != null)
            {
                try
                {
                    _context.FAQs.Remove(faq);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "SSS öğesi başarıyla silindi.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "SSS öğesi silinirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                TempData["Error"] = "SSS öğesi bulunamadı.";
            }

            return RedirectToAction(nameof(FAQ));
        }

        // GET: Admin/Videos
        public async Task<IActionResult> Videos()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var videos = await _context.Videos
                    .OrderBy(v => v.DisplayOrder)
                    .ThenByDescending(v => v.CreatedAt)
                    .ToListAsync();

                // Null değerleri güvenli hale getir
                foreach (var video in videos)
                {
                    video.Title = video.Title ?? "";
                    video.Description = video.Description ?? "";
                    video.VideoUrl = video.VideoUrl ?? "";
                    video.ThumbnailUrl = video.ThumbnailUrl ?? "";
                }

                return View(videos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading videos");
                TempData["Error"] = "Videolar yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Video>());
            }
        }

        // GET: Admin/AddVideo
        public IActionResult AddVideo()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Admin/AddVideo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVideo(Video video)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    video.CreatedAt = DateTimeHelper.NowTurkey;
                    video.IsActive = true;
                    video.ViewCount = 0;

                    _context.Videos.Add(video);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Video başarıyla eklendi.";
                    return RedirectToAction(nameof(Videos));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Video eklenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(video);
        }

        // GET: Admin/EditVideo/5
        public async Task<IActionResult> EditVideo(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var video = await _context.Videos.FindAsync(id);
                if (video == null)
                {
                    TempData["Error"] = "Video bulunamadı.";
                    return RedirectToAction(nameof(Videos));
                }

                return View(video);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Video yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Videos));
            }
        }

        // POST: Admin/EditVideo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVideo(int id, Video video)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (id != video.Id)
                {
                    TempData["Error"] = "Video ID'si uyuşmuyor.";
                    return RedirectToAction(nameof(Videos));
                }

                if (ModelState.IsValid)
                {
                    var existingVideo = await _context.Videos.FindAsync(id);
                    if (existingVideo == null)
                    {
                        TempData["Error"] = "Video bulunamadı.";
                        return RedirectToAction(nameof(Videos));
                    }

                    // Video özelliklerini güncelle
                    existingVideo.Title = video.Title;
                    existingVideo.Description = video.Description;
                    existingVideo.VideoUrl = video.VideoUrl;
                    existingVideo.ThumbnailUrl = video.ThumbnailUrl;
                    existingVideo.Duration = video.Duration;
                    existingVideo.IsActive = video.IsActive;
                    existingVideo.IsFeatured = video.IsFeatured;
                    existingVideo.DisplayOrder = video.DisplayOrder;
                    existingVideo.UpdatedAt = DateTimeHelper.NowTurkey;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Video başarıyla güncellendi.";
                    return RedirectToAction(nameof(Videos));
                }
                else
                {
                    return View(video);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Video güncellenirken bir hata oluştu: " + ex.Message);
                return View(video);
            }
        }

        // POST: Admin/DeleteVideo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var video = await _context.Videos.FindAsync(id);
            if (video != null)
            {
                try
                {
                    _context.Videos.Remove(video);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Video başarıyla silindi.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Video silinirken bir hata oluştu: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Videos));
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                // Null değerleri güvenli hale getir
                foreach (var category in categories)
                {
                    category.Name = category.Name ?? "";
                    category.Description = category.Description ?? "";
                }

                // Ürün sayılarını hesapla
                var productCounts = await _context.Products
                    .Where(p => p.CategoryId.HasValue)
                    .GroupBy(p => p.CategoryId!.Value)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

                ViewBag.ProductCounts = productCounts;

                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                TempData["Error"] = "Kategoriler yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Category>());
            }
        }

        // POST: Admin/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTimeHelper.NowTurkey;
                category.IsActive = true;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kategori başarıyla eklendi.";
            }
            else
            {
                TempData["Error"] = "Kategori eklenirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Categories));
        }

        // POST: Admin/EditCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var existingCategory = await _context.Categories.FindAsync(category.Id);
                    if (existingCategory == null)
                    {
                        TempData["Error"] = "Kategori bulunamadı.";
                        return RedirectToAction(nameof(Categories));
                    }

                    // Kategori özelliklerini güncelle
                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.DisplayOrder = category.DisplayOrder;
                    existingCategory.IsActive = category.IsActive;
                    existingCategory.UpdatedAt = DateTimeHelper.NowTurkey;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Kategori başarıyla güncellendi.";
                }
                else
                {
                    TempData["Error"] = "Kategori güncellenirken bir hata oluştu.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kategori güncellenirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Categories));
        }

        // POST: Admin/DeleteCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                try
                {
                    // Kategoriye ait ürün var mı kontrol et
                    var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
                    if (hasProducts)
                    {
                        TempData["Error"] = "Bu kategoriye ait ürünler bulunduğu için silinemez.";
                    }
                    else
                    {
                        category.IsActive = false;
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Kategori başarıyla silindi.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Kategori silinirken bir hata oluştu: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Categories));
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Orders()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                // Null değerleri güvenli hale getir
                foreach (var order in orders)
                {
                    if (order.User != null)
                    {
                        order.User.FirstName = order.User.FirstName ?? "";
                        order.User.LastName = order.User.LastName ?? "";
                        order.User.Email = order.User.Email ?? "";
                        order.User.Phone = order.User.Phone ?? "";
                    }
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                TempData["Error"] = "Siparişler yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Order>());
            }
        }

        // GET: Admin/OrderDetail/5
        public async Task<IActionResult> OrderDetail(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Null değerleri güvenli hale getir
                if (order.User != null)
                {
                    order.User.FirstName = order.User.FirstName ?? "";
                    order.User.LastName = order.User.LastName ?? "";
                    order.User.Email = order.User.Email ?? "";
                    order.User.Phone = order.User.Phone ?? "";
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order detail for ID: {OrderId}", id);
                return NotFound();
            }
        }

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    // Sipariş durumunu güncelle
                    order.OrderStatus = status;

                    if (status == ApplicationConstants.OrderStatus.Shipped)
                    {
                        order.ShippedDate = DateTimeHelper.NowTurkey;
                    }
                    else if (status == ApplicationConstants.OrderStatus.Delivered)
                    {
                        order.DeliveredDate = DateTimeHelper.NowTurkey;
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Sipariş durumu başarıyla güncellendi.";
                }
                else
                {
                    TempData["Error"] = "Sipariş bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Sipariş durumu güncellenirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Orders));
        }

        // GET: Admin/Messages
        public async Task<IActionResult> Messages()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var messages = await _context.ContactMessages
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();

                // Null değerleri güvenli hale getir
                foreach (var message in messages)
                {
                    message.Name = message.Name ?? "";
                    message.Email = message.Email ?? "";
                    message.Subject = message.Subject ?? "";
                    message.Message = message.Message ?? "";
                    message.ReplyMessage = message.ReplyMessage ?? "";
                }

                return View(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading messages");
                TempData["Error"] = "Mesajlar yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<ContactMessage>());
            }
        }

        // GET: Admin/MessageDetail/5
        public async Task<IActionResult> MessageDetail(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            // Mesajı okundu olarak işaretle
            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        // POST: Admin/ReplyMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyMessage(int messageId, string replyMessage)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var message = await _context.ContactMessages.FindAsync(messageId);
                if (message != null)
                {
                    // Mesaj yanıtını güncelle
                    message.IsReplied = true;
                    message.RepliedAt = DateTimeHelper.NowTurkey;
                    message.ReplyMessage = replyMessage;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Yanıt başarıyla gönderildi.";
                }
                else
                {
                    TempData["Error"] = "Mesaj bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Yanıt gönderilirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Messages));
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Null değerleri güvenli şekilde handle etmek için Select kullan
                var users = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => new
                    {
                        u.Id,
                        FirstName = u.FirstName ?? "",
                        LastName = u.LastName ?? "",
                        Email = u.Email ?? "",
                        Phone = u.Phone ?? "",
                        Address = u.Address ?? "",
                        City = u.City ?? "",
                        PostalCode = u.PostalCode ?? "",
                        u.IsActive,
                        u.IsAdmin,
                        u.EmailConfirmed,
                        u.CreatedAt,
                        u.UpdatedAt,
                        u.LastLoginAt,
                        PasswordResetToken = u.PasswordResetToken ?? "",
                        PasswordResetCode = u.PasswordResetCode ?? "",
                        GoogleId = u.GoogleId ?? ""
                    })
                    .ToListAsync();

                // Anonim tipi User nesnesine dönüştür
                var userList = users.Select(u => new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    City = u.City,
                    PostalCode = u.PostalCode,
                    IsActive = u.IsActive,
                    IsAdmin = u.IsAdmin,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LastLoginAt = u.LastLoginAt,
                    PasswordResetToken = u.PasswordResetToken,
                    PasswordResetCode = u.PasswordResetCode,
                    GoogleId = u.GoogleId
                }).ToList();

                return View(userList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["Error"] = "Kullanıcılar yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<User>());
            }
        }

        // GET: Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                // Null değerleri güvenli hale getir
                user.FirstName = user.FirstName ?? "";
                user.LastName = user.LastName ?? "";
                user.Email = user.Email ?? "";
                user.Phone = user.Phone ?? "";
                user.Address = user.Address ?? "";
                user.City = user.City ?? "";
                user.PostalCode = user.PostalCode ?? "";
                user.PasswordResetToken = user.PasswordResetToken ?? "";
                user.PasswordResetCode = user.PasswordResetCode ?? "";
                user.GoogleId = user.GoogleId ?? "";

                return PartialView("_UserDetails", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for ID: {UserId}", id);
                return NotFound();
            }
        }

        // POST: Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User user)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var existingUser = await _context.Users.FindAsync(user.Id);
                    if (existingUser == null)
                    {
                        TempData["Error"] = "Kullanıcı bulunamadı.";
                        return RedirectToAction(nameof(Users));
                    }

                    // Null değerleri güvenli hale getir
                    user.FirstName = user.FirstName ?? "";
                    user.LastName = user.LastName ?? "";
                    user.Email = user.Email ?? "";
                    user.Phone = user.Phone ?? "";

                    // Kullanıcı özelliklerini güncelle
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.Phone = user.Phone;
                    existingUser.IsAdmin = user.IsAdmin;
                    existingUser.IsActive = user.IsActive;
                    existingUser.UpdatedAt = DateTimeHelper.NowTurkey;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                }
                else
                {
                    TempData["Error"] = "Kullanıcı güncellenirken bir hata oluştu.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", user.Id);
                TempData["Error"] = "Kullanıcı güncellenirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int userId, bool isActive)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    // Kullanıcı durumunu güncelle
                    user.IsActive = isActive;
                    user.UpdatedAt = DateTimeHelper.NowTurkey;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Kullanıcı başarıyla {(isActive ? "aktif" : "pasif")} yapıldı.";
                }
                else
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kullanıcı durumu güncellenirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Users));
        }

        [AdminAuthorization]
        public IActionResult Performance()
        {
            try
            {
                var performanceService = HttpContext.RequestServices.GetService<IPerformanceMonitorService>();
                var metrics = performanceService?.GetPerformanceMetrics() ?? new Dictionary<string, object>();

                ViewBag.PerformanceMetrics = metrics;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading performance metrics");
                TempData["ErrorMessage"] = "Performans metrikleri yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }

        private bool GalleryExists(int id)
        {
            return _context.Galleries.Any(e => e.Id == id);
        }

        private bool FAQExists(int id)
        {
            return _context.FAQs.Any(e => e.Id == id);
        }

        private bool VideoExists(int id)
        {
            return _context.Videos.Any(e => e.Id == id);
        }

        // ============== ABOUT CONTENT MANAGEMENT ==============

        // GET: Admin/AboutContent
        public async Task<IActionResult> AboutContent()
        {
            // Geçici olarak admin kontrolünü kaldır
            // if (!IsAdmin())
            // {
            //     return RedirectToAction("Login", "Account");
            // }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var aboutService = scope.ServiceProvider.GetService<manyasligida.Services.Interfaces.IAboutService>();

                if (aboutService != null)
                {
                    var aboutResult = await aboutService.GetAboutContentAsync();
                    if (aboutResult.Success && aboutResult.Data != null)
                    {
                        return View(aboutResult.Data);
                    }
                }

                // Varsayılan içerik
                var defaultContent = new manyasligida.Models.DTOs.AboutContentResponse
                {
                    Title = "Hakkımızda",
                    Subtitle = "Gelenekten geleceğe kaliteli lezzetlerin hikayesi",
                    StoryTitle = "Hikayemiz",
                    StorySubtitle = "40 Yıllık Deneyim",
                    StoryContent = "1980 yılında Manyas'ta küçük bir mandıra olarak başladığımız yolculuğumuzda, bugün Türkiye'nin önde gelen süt ürünleri üreticilerinden biri haline geldik. Geleneksel yöntemlerle modern teknolojiyi birleştirerek, en kaliteli ve lezzetli ürünleri üretiyoruz.",
                    StoryImageUrl = "/img/about-story.jpg",
                    RegionTitle = "Manyas Bölgesi",
                    RegionSubtitle = "Doğal Zenginlikler",
                    RegionContent = "Manyas'ın bereketli toprakları ve temiz havası, süt ürünlerimizin kalitesini artıran en önemli faktörlerden biridir. Bölgemizin doğal zenginlikleri, ürünlerimizin benzersiz lezzetini oluşturur.",
                    RegionImageUrl = "/img/about-region.jpg",
                    ValuesTitle = "Değerlerimiz",
                    ValuesSubtitle = "Kalite ve Güven",
                    ValuesContent = "Müşteri memnuniyeti, kalite, güven ve sürdürülebilirlik temel değerlerimizdir. Her ürünümüzde bu değerleri yansıtmaya özen gösteriyoruz.",
                    MissionTitle = "Misyonumuz",
                    MissionContent = "Geleneksel lezzetleri modern teknoloji ile birleştirerek, en kaliteli süt ürünlerini müşterilerimize sunmak.",
                    VisionTitle = "Vizyonumuz",
                    VisionContent = "Türkiye'nin en güvenilir ve tercih edilen süt ürünleri markası olmak."
                };

                return View(defaultContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading about content");
                TempData["Error"] = "Hakkımızda içeriği yüklenirken bir hata oluştu";
                return View(new manyasligida.Models.DTOs.AboutContentResponse());
            }
        }

        // POST: Admin/EditAboutContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAboutContent(manyasligida.Models.DTOs.AboutContentResponse model, IFormFile? storyImageFile, IFormFile? regionImageFile)
        {
            // Geçici olarak admin kontrolünü kaldır
            // if (!IsAdmin())
            // {
            //     return RedirectToAction("Login", "Account");
            // }

            try
            {
                if (ModelState.IsValid)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var aboutService = scope.ServiceProvider.GetService<manyasligida.Services.Interfaces.IAboutService>();

                    if (aboutService != null)
                    {
                        // Resim yükleme işlemleri
                        if (storyImageFile != null && storyImageFile.Length > 0)
                        {
                            if (!_fileUploadService.IsValidImage(storyImageFile))
                            {
                                ModelState.AddModelError("StoryImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                                return View("AboutContent", model);
                            }

                            var storyImageUrl = await _fileUploadService.UploadImageAsync(storyImageFile, "about");
                            model = model with { StoryImageUrl = storyImageUrl };
                        }

                        if (regionImageFile != null && regionImageFile.Length > 0)
                        {
                            if (!_fileUploadService.IsValidImage(regionImageFile))
                            {
                                ModelState.AddModelError("RegionImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                                return View("AboutContent", model);
                            }

                            var regionImageUrl = await _fileUploadService.UploadImageAsync(regionImageFile, "about");
                            model = model with { RegionImageUrl = regionImageUrl };
                        }

                        var result = await aboutService.UpdateAboutContentAsync(new manyasligida.Models.DTOs.AboutEditRequest
                        {
                            Title = model.Title,
                            Subtitle = model.Subtitle,
                            StoryTitle = model.StoryTitle,
                            StorySubtitle = model.StorySubtitle,
                            StoryContent = model.StoryContent,
                            StoryImageUrl = model.StoryImageUrl,
                            MissionTitle = model.MissionTitle,
                            MissionContent = model.MissionContent,
                            VisionTitle = model.VisionTitle,
                            VisionContent = model.VisionContent,
                            ValuesTitle = model.ValuesTitle,
                            ValuesSubtitle = model.ValuesSubtitle,
                            ValuesContent = model.ValuesContent,
                            RegionTitle = model.RegionTitle,
                            RegionSubtitle = model.RegionSubtitle,
                            RegionContent = model.RegionContent,
                            RegionImageUrl = model.RegionImageUrl
                        });

                        if (result.Success)
                        {
                            TempData["Success"] = "Hakkımızda içeriği başarıyla güncellendi.";
                            return RedirectToAction("AboutContent");
                        }
                        else
                        {
                            TempData["Error"] = result.Message ?? "Hakkımızda içeriği güncellenirken bir hata oluştu.";
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Hakkımızda servisi bulunamadı.";
                    }
                }
                else
                {
                    TempData["Error"] = "Geçersiz veri. Lütfen tüm alanları kontrol edin.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating about content");
                TempData["Error"] = "Hakkımızda içeriği güncellenirken bir hata oluştu: " + ex.Message;
            }

            return View("AboutContent", model);
        }

        // POST: Admin/ResetAboutContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorization]
        public async Task<IActionResult> ResetAboutContent()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var aboutService = scope.ServiceProvider.GetRequiredService<manyasligida.Services.Interfaces.IAboutService>();

                // Get current content and deactivate it
                var currentResult = await aboutService.GetAboutContentAsync();
                if (currentResult.Success && currentResult.Data != null)
                {
                    await aboutService.DeleteAboutContentAsync(currentResult.Data.Id);
                }

                // Create new default content
                var result = await aboutService.CreateDefaultAboutContentAsync();

                if (result.Success)
                {
                    TempData["Success"] = "About content varsayılan değerlere sıfırlandı!";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(EditAboutContent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting about content");
                TempData["Error"] = "About content sıfırlanırken hata oluştu.";
                return RedirectToAction(nameof(EditAboutContent));
            }
        }

        private bool AboutContentExists(int id)
        {
            return _context.AboutContents.Any(e => e.Id == id);
        }

        // ============== HOME CONTENT MANAGEMENT ==============

        // GET: Admin/HomeContent
        public async Task<IActionResult> HomeContent()
        {
            // Geçici olarak admin kontrolünü kaldır
            // if (!await IsAdminAsync())
            // {
            //     return RedirectToAction("Login", "Account");
            // }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var homeService = scope.ServiceProvider.GetService<manyasligida.Services.Interfaces.IHomeService>();

                if (homeService != null)
                {
                    var homeResult = await homeService.GetHomeContentAsync();
                    if (homeResult.Success && homeResult.Data != null)
                    {
                        return View(homeResult.Data);
                    }
                }

                // Varsayılan anasayfa içeriği
                var defaultContent = new manyasligida.Models.DTOs.HomeContentResponse
                {
                    HeroTitle = "Kaliteli Lezzetin Adresi",
                    HeroSubtitle = "Uzman ellerden sofralarınıza uzanan kaliteli süt ürünleri",
                    HeroDescription = "Taze ve güvenilir üretimle sağlığınız için en iyisi.",
                    HeroButtonText = "Ürünleri Keşfet",
                    HeroButtonUrl = "/Products",
                    HeroImageUrl = "/img/carousel-1.jpg",
                    FeaturesTitle = "Neden Bizi Tercih Etmelisiniz?",
                    FeaturesSubtitle = "Kalite, güven ve lezzet bir arada",
                    ProductsTitle = "Popüler Ürünlerimiz",
                    ProductsSubtitle = "En çok tercih edilen lezzetler",
                    ShowPopularProducts = true,
                    MaxProductsToShow = 8,
                    StatsTitle = "Güvenin Rakamları",
                    StatsSubtitle = "38 yıldır devam eden kalite yolculuğumuz",
                    BlogTitle = "Son Haberler & Blog",
                    BlogSubtitle = "Sektörden son gelişmeler ve öneriler",
                    ShowLatestBlogs = true,
                    MaxBlogsToShow = 3,
                    NewsletterTitle = "Haberdar Ol!",
                    NewsletterDescription = "Yeni ürünler, kampanyalar ve özel fırsatlardan ilk sen haberdar ol.",
                    NewsletterButtonText = "Abone Ol",
                    ContactTitle = "İletişim",
                    ContactSubtitle = "Bizimle İletişime Geçin",
                    ContactDescription = "Sorularınız için bize ulaşabilir, özel siparişlerinizi verebilirsiniz.",
                    ContactPhone = "+90 266 123 45 67",
                    ContactEmail = "info@manyasligida.com",
                    ContactAddress = "Manyas, Balıkesir, Türkiye",
                    HeroBackgroundColor = "linear-gradient(135deg, #8b4513 0%, #d2691e 100%)",
                    PrimaryColor = "#8B4513",
                    SecondaryColor = "#D2691E"
                };

                return View(defaultContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home content");
                TempData["Error"] = "Anasayfa içeriği yüklenirken bir hata oluştu";
                return View(new manyasligida.Models.DTOs.HomeContentResponse());
            }
        }

        // POST: Admin/EditHomeContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHomeContent(manyasligida.Models.DTOs.HomeContentResponse model, IFormFile? heroImageFile, IFormFile? aboutImageFile, IFormFile? heroVideoFile)
        {
            // Geçici olarak admin kontrolünü kaldır
            // if (!await IsAdminAsync())
            // {
            //     return RedirectToAction("Login", "Account");
            // }

            try
            {
                if (ModelState.IsValid)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var homeService = scope.ServiceProvider.GetService<manyasligida.Services.Interfaces.IHomeService>();

                    if (homeService != null)
                    {
                        // Video yükleme işlemi
                        if (heroVideoFile != null && heroVideoFile.Length > 0)
                        {
                            if (!_fileUploadService.IsValidVideo(heroVideoFile))
                            {
                                ModelState.AddModelError("HeroVideoFile", "Geçersiz video formatı. Sadece MP4, AVI, MOV dosyaları kabul edilir.");
                                return View("HomeContent", model);
                            }

                            var heroVideoUrl = await _fileUploadService.UploadVideoAsync(heroVideoFile, "home");
                            model = model with { HeroVideoUrl = heroVideoUrl };
                        }

                        // Resim yükleme işlemleri
                        if (heroImageFile != null && heroImageFile.Length > 0)
                        {
                            if (!_fileUploadService.IsValidImage(heroImageFile))
                            {
                                ModelState.AddModelError("HeroImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                                return View("HomeContent", model);
                            }

                            var heroImageUrl = await _fileUploadService.UploadImageAsync(heroImageFile, "home");
                            model = model with { HeroImageUrl = heroImageUrl };
                        }

                        if (aboutImageFile != null && aboutImageFile.Length > 0)
                        {
                            if (!_fileUploadService.IsValidImage(aboutImageFile))
                            {
                                ModelState.AddModelError("AboutImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                                return View("HomeContent", model);
                            }

                            var aboutImageUrl = await _fileUploadService.UploadImageAsync(aboutImageFile, "home");
                            model = model with { AboutImageUrl = aboutImageUrl };
                        }

                        var result = await homeService.UpdateHomeContentAsync(new manyasligida.Models.DTOs.HomeEditRequest
                        {
                            HeroTitle = model.HeroTitle,
                            HeroSubtitle = model.HeroSubtitle,
                            HeroDescription = model.HeroDescription,
                            HeroVideoUrl = model.HeroVideoUrl,
                            HeroImageUrl = model.HeroImageUrl,
                            HeroButtonText = model.HeroButtonText,
                            HeroButtonUrl = model.HeroButtonUrl,
                            HeroSecondButtonText = model.HeroSecondButtonText,
                            HeroBackgroundColor = model.HeroBackgroundColor,
                            FeaturesTitle = model.FeaturesTitle,
                            FeaturesSubtitle = model.FeaturesSubtitle,
                            ProductsTitle = model.ProductsTitle,
                            ProductsSubtitle = model.ProductsSubtitle,
                            ShowPopularProducts = model.ShowPopularProducts,
                            MaxProductsToShow = model.MaxProductsToShow,
                            StatsTitle = model.StatsTitle,
                            StatsSubtitle = model.StatsSubtitle,
                            BlogTitle = model.BlogTitle,
                            BlogSubtitle = model.BlogSubtitle,
                            ShowLatestBlogs = model.ShowLatestBlogs,
                            MaxBlogsToShow = model.MaxBlogsToShow,
                            NewsletterTitle = model.NewsletterTitle,
                            NewsletterDescription = model.NewsletterDescription,
                            NewsletterButtonText = model.NewsletterButtonText,
                            ContactTitle = model.ContactTitle,
                            ContactSubtitle = model.ContactSubtitle,
                            ContactDescription = model.ContactDescription,
                            ContactPhone = model.ContactPhone,
                            ContactEmail = model.ContactEmail,
                            ContactAddress = model.ContactAddress,
                            PrimaryColor = model.PrimaryColor,
                            SecondaryColor = model.SecondaryColor
                        });

                        if (result.Success)
                        {
                            TempData["Success"] = "Ana sayfa içeriği başarıyla güncellendi.";
                            return RedirectToAction("HomeContent");
                        }
                        else
                        {
                            TempData["Error"] = result.Message ?? "Ana sayfa içeriği güncellenirken bir hata oluştu.";
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Ana sayfa servisi bulunamadı.";
                    }
                }
                else
                {
                    TempData["Error"] = "Geçersiz veri. Lütfen tüm alanları kontrol edin.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating home content");
                TempData["Error"] = "Ana sayfa içeriği güncellenirken bir hata oluştu: " + ex.Message;
            }

            return View("HomeContent", model);
        }

        // POST: Admin/ResetHomeContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorization]
        public async Task<IActionResult> ResetHomeContent()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var homeService = scope.ServiceProvider.GetRequiredService<manyasligida.Services.Interfaces.IHomeService>();

                // Get current content and deactivate it
                var currentResult = await homeService.GetHomeContentAsync();
                if (currentResult.Success && currentResult.Data != null)
                {
                    await homeService.DeleteHomeContentAsync(currentResult.Data.Id);
                }

                // Create new default content
                var result = await homeService.CreateDefaultHomeContentAsync();

                if (result.Success)
                {
                    TempData["Success"] = "Ana sayfa içeriği varsayılan değerlere sıfırlandı!";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(HomeContent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting home content");
                TempData["Error"] = "Ana sayfa içeriği sıfırlanırken hata oluştu.";
                return RedirectToAction(nameof(HomeContent));
            }
        }

        private bool HomeContentExists(int id)
        {
            return _context.HomeContents.Any(e => e.Id == id);
        }

        // GET: Admin/SiteSettings
        [AdminAuthorization]
        public async Task<IActionResult> SiteSettings()
        {
            try
            {
                var settings = await _siteSettingsService.GetAsync();
                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading site settings");
                TempData["Error"] = "Site ayarları yüklenirken bir hata oluştu.";
                return View(new SiteSettings());
            }
        }

        // POST: Admin/SiteSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorization]
        public async Task<IActionResult> SiteSettings(SiteSettings settings, IFormFile? logoFile, IFormFile? faviconFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Logo yükleme
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(logoFile))
                        {
                            ModelState.AddModelError("LogoFile", "Geçersiz logo dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            return View(settings);
                        }

                        var logoUrl = await _fileUploadService.UploadImageAsync(logoFile, "site");
                        settings.LogoUrl = logoUrl;
                    }

                    // Favicon yükleme
                    if (faviconFile != null && faviconFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(faviconFile))
                        {
                            ModelState.AddModelError("FaviconFile", "Geçersiz favicon dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            return View(settings);
                        }

                        var faviconUrl = await _fileUploadService.UploadImageAsync(faviconFile, "site");
                        settings.FaviconUrl = faviconUrl;
                    }

                    var success = await _siteSettingsService.UpdateAsync(settings);
                    if (success)
                    {
                        TempData["Success"] = "Site ayarları başarıyla güncellendi.";
                        return RedirectToAction(nameof(SiteSettings));
                    }
                    else
                    {
                        TempData["Error"] = "Site ayarları güncellenirken bir hata oluştu.";
                    }
                }
                else
                {
                    TempData["Error"] = "Lütfen tüm gerekli alanları doldurun.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating site settings");
                TempData["Error"] = "Site ayarları güncellenirken bir hata oluştu: " + ex.Message;
            }

            return View(settings);
        }
    }
}