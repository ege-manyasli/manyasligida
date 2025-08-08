using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace manyasligida.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IAuthService _authService;

        public AdminController(ApplicationDbContext context, CartService cartService, IFileUploadService fileUploadService, IAuthService authService)
        {
            _context = context;
            _cartService = cartService;
            _fileUploadService = fileUploadService;
            _authService = authService;
        }

        // Admin kontrolü
        private async Task<bool> IsAdminAsync()
        {
            return await _authService.IsCurrentUserAdminAsync();
        }

        // Sync admin check for backward compatibility
        private bool IsAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return isAdmin == "True";
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            // Eğer zaten admin girişi yapılmışsa dashboard'a yönlendir
            if (IsAdmin())
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
                var user = await _authService.LoginAsync(model.Email, model.Password);
                if (user != null && user.IsAdmin)
                {
                    TempData["LoginSuccess"] = "Admin paneline başarıyla giriş yaptınız!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Admin yetkisi bulunmayan kullanıcı.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Giriş sırasında bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View(model);
        }

        // GET: Admin/Index (Ana admin sayfası)
        public async Task<IActionResult> Index()
        {
            if (!await IsAdminAsync())
            {
                return RedirectToAction("Login", "Account");
            }

            // Dashboard istatistikleri
            var totalProducts = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalBlogs = await _context.Blogs.CountAsync();
            var totalGalleries = await _context.Galleries.CountAsync();
            var totalVideos = await _context.Videos.CountAsync();
            var unreadMessages = await _context.ContactMessages.Where(m => !m.IsRead).CountAsync();
            var totalRevenue = await _context.Orders.Where(o => o.OrderStatus == ApplicationConstants.OrderStatus.Delivered).SumAsync(o => o.TotalAmount);

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
                    CustomerName = $"{o.User.FirstName} {o.User.LastName}",
                    CustomerEmail = o.User.Email,
                    o.TotalAmount,
                    o.OrderStatus,
                    o.OrderDate
                })
                .ToListAsync();

            // Son mesajlar
            var recentMessages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentOrders = recentOrders;
            ViewBag.RecentMessages = recentMessages;

            return View();
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

                return View(products);
            }
            catch (Exception ex)
            {
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

                        // Minimum çözünürlük kontrolü (örnek: 1200x1200)
                        var okRes = await _fileUploadService.IsResolutionSufficientAsync(imageFile, 1200, 1200);
                        if (!okRes)
                        {
                            ModelState.AddModelError("ImageFile", "Görsel çözünürlüğü düşük. Lütfen en az 1200x1200 piksel görsel yükleyin.");
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
                                // Galeri için de temel çözünürlük kontrolü (örnek: 1000x1000)
                                var okGallery = await _fileUploadService.IsResolutionSufficientAsync(gf, 1000, 1000);
                                if (!okGallery) continue;
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
                    product.CreatedAt = DateTime.Now;
                    product.IsActive = true;
                    product.UpdatedAt = DateTime.Now;

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

                        var okRes = await _fileUploadService.IsResolutionSufficientAsync(imageFile, 1200, 1200);
                        if (!okRes)
                        {
                            ModelState.AddModelError("ImageFile", "Görsel çözünürlüğü düşük. Lütfen en az 1200x1200 piksel görsel yükleyin.");
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
                                var okGallery = await _fileUploadService.IsResolutionSufficientAsync(gf, 1000, 1000);
                                if (!okGallery) continue;
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
                    product.UpdatedAt = DateTime.Now;
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
                    // Ürün resmini sil
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        await _fileUploadService.DeleteImageAsync(product.ImageUrl);
                    }

                    // Ürünü tamamen sil
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ürün başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Ürün bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ürün silinirken bir hata oluştu: " + ex.Message;
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
                var blogs = await _context.Blogs
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                return View(blogs);
            }
            catch (Exception ex)
            {
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

                    blog.CreatedAt = DateTime.Now;
                    blog.IsActive = true;

                    _context.Blogs.Add(blog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Blog yazısı başarıyla eklendi.";
                    return RedirectToAction(nameof(Blog));
                }
                catch (Exception ex)
                {
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
                        blog.ImageUrl = existingBlog.ImageUrl;
                    }

                    // Blog özelliklerini güncelle
                    blog.UpdatedAt = DateTime.Now;
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

            var galleries = await _context.Galleries
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return View(galleries);
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

                    gallery.CreatedAt = DateTime.Now;
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
                    gallery.UpdatedAt = DateTime.Now;
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

            var faqs = await _context.FAQs
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            return View(faqs);
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
                faq.CreatedAt = DateTime.Now;
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
                    faq.UpdatedAt = DateTime.Now;
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

                return View(videos);
            }
            catch (Exception ex)
            {
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
                    video.CreatedAt = DateTime.Now;
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
                    existingVideo.UpdatedAt = DateTime.Now;

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

            var categories = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Ürün sayılarını hesapla
            var productCounts = await _context.Products
                .Where(p => p.CategoryId.HasValue)
                .GroupBy(p => p.CategoryId!.Value)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            ViewBag.ProductCounts = productCounts;

            return View(categories);
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
                category.CreatedAt = DateTime.Now;
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
                    existingCategory.UpdatedAt = DateTime.Now;

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

            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/OrderDetail/5
        public async Task<IActionResult> OrderDetail(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
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
                        order.ShippedDate = DateTime.Now;
                    }
                    else if (status == ApplicationConstants.OrderStatus.Delivered)
                    {
                        order.DeliveredDate = DateTime.Now;
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

            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(messages);
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
                    message.RepliedAt = DateTime.Now;
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

            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // GET: Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return PartialView("_UserDetails", user);
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

                    // Kullanıcı özelliklerini güncelle
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.Phone = user.Phone;
                    existingUser.IsAdmin = user.IsAdmin;
                    existingUser.IsActive = user.IsActive;
                    existingUser.UpdatedAt = DateTime.Now;

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
                    user.UpdatedAt = DateTime.Now;

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
    }
} 