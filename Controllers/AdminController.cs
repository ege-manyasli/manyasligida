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

        public AdminController(ApplicationDbContext context, CartService cartService, IFileUploadService fileUploadService)
        {
            _context = context;
            _cartService = cartService;
            _fileUploadService = fileUploadService;
        }

        // Admin kontrolü
        private bool IsAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return isAdmin == "True";
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            if (IsAdmin())
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string password)
        {
            // Admin şifresi (gerçek uygulamada bu şifre güvenli bir şekilde saklanmalıdır)
            const string ADMIN_PASSWORD = "admin123";

            if (password == ADMIN_PASSWORD)
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.IsAdmin);
                if (adminUser != null)
                {
                    HttpContext.Session.SetString("UserId", adminUser.Id.ToString());
                    HttpContext.Session.SetString("UserName", $"{adminUser.FirstName} {adminUser.LastName}");
                    HttpContext.Session.SetString("UserEmail", adminUser.Email);
                    HttpContext.Session.SetString("IsAdmin", "True");

                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Error = "Geçersiz şifre!";
            return View();
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
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
            var unreadMessages = await _context.ContactMessages.Where(m => !m.IsRead).CountAsync();
            var totalRevenue = await _context.Orders.Where(o => o.OrderStatus == "Delivered").SumAsync(o => o.TotalAmount);

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalBlogs = totalBlogs;
            ViewBag.TotalGalleries = totalGalleries;
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
                // Debug: Log the query
                System.Diagnostics.Debug.WriteLine("Loading products from database...");
                
                var products = await _context.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Debug: Log the results
                System.Diagnostics.Debug.WriteLine($"Found {products.Count} products in database");
                foreach (var product in products)
                {
                    System.Diagnostics.Debug.WriteLine($"Product: {product.Name} (ID: {product.Id}, Category: {product.Category?.Name})");
                }

                return View(products);
            }
            catch (Exception ex)
            {
                // Debug: Log the exception
                System.Diagnostics.Debug.WriteLine($"Exception in Products action: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Exception Stack Trace: {ex.StackTrace}");
                
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

        // POST: Admin/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFile? imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            // Debug: Log the incoming product data
            System.Diagnostics.Debug.WriteLine($"Product Name: {product.Name}");
            System.Diagnostics.Debug.WriteLine($"Product Price: {product.Price}");
            System.Diagnostics.Debug.WriteLine($"Product CategoryId: {product.CategoryId}");
            System.Diagnostics.Debug.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Debug: Log ModelState errors
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
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
                            var activeCategories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                            ViewBag.Categories = activeCategories;
                            return View(product);
                        }

                        var imageUrl = await _fileUploadService.UploadImageAsync(imageFile, "products");
                        product.ImageUrl = imageUrl;
                    }

                    product.CreatedAt = DateTime.Now;
                    product.IsActive = true;

                    // Debug: Log before saving
                    System.Diagnostics.Debug.WriteLine($"About to save product: {product.Name}");

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    // Debug: Log after saving
                    System.Diagnostics.Debug.WriteLine($"Product saved successfully with ID: {product.Id}");

                    TempData["Success"] = "Ürün başarıyla eklendi.";
                    return RedirectToAction(nameof(Products));
                }
                catch (Exception ex)
                {
                    // Debug: Log the exception details
                    System.Diagnostics.Debug.WriteLine($"Exception in AddProduct: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception Stack Trace: {ex.StackTrace}");
                    
                    ModelState.AddModelError("", "Ürün eklenirken bir hata oluştu: " + ex.Message);
                }
            }

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Categories = categories;

            return View(product);
        }

        // GET: Admin/EditProduct/5
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Categories = categories;

            return View(product);
        }

        // POST: Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut ürünü getir
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Yeni resim yüklendiyse
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!_fileUploadService.IsValidImage(imageFile))
                        {
                            ModelState.AddModelError("ImageFile", "Geçersiz dosya formatı. Sadece JPG, PNG, GIF ve WEBP dosyaları kabul edilir.");
                            var activeCategories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                            ViewBag.Categories = activeCategories;
                            return View(product);
                        }

                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            await _fileUploadService.DeleteImageAsync(existingProduct.ImageUrl);
                        }

                        // Yeni resmi yükle
                        var imageUrl = await _fileUploadService.UploadImageAsync(imageFile, "products");
                        product.ImageUrl = imageUrl;
                    }
                    else
                    {
                        // Resim değişmediyse mevcut resmi koru
                        product.ImageUrl = existingProduct.ImageUrl;
                    }

                    product.UpdatedAt = DateTime.Now;
                    product.CreatedAt = existingProduct.CreatedAt; // Mevcut oluşturma tarihini koru

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction(nameof(Products));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ürün güncellenirken bir hata oluştu: " + ex.Message);
                }
            }

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Categories = categories;

            return View(product);
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

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                try
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
                catch (Exception ex)
                {
                    TempData["Error"] = "Ürün silinirken bir hata oluştu: " + ex.Message;
                }
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

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
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

            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut blog'u getir
                    var existingBlog = await _context.Blogs.FindAsync(id);
                    if (existingBlog == null)
                    {
                        return NotFound();
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

                    blog.UpdatedAt = DateTime.Now;
                    blog.CreatedAt = existingBlog.CreatedAt; // Mevcut oluşturma tarihini koru

                    _context.Update(blog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Blog yazısı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Blog));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Blog yazısı güncellenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(blog);
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

            var gallery = await _context.Galleries.FindAsync(id);
            if (gallery == null)
            {
                return NotFound();
            }

            return View(gallery);
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

            if (id != gallery.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut galeri öğesini getir
                    var existingGallery = await _context.Galleries.FindAsync(id);
                    if (existingGallery == null)
                    {
                        return NotFound();
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

                    gallery.UpdatedAt = DateTime.Now;
                    gallery.CreatedAt = existingGallery.CreatedAt; // Mevcut oluşturma tarihini koru

                    _context.Update(gallery);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Galeri öğesi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Gallery));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GalleryExists(gallery.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Galeri öğesi güncellenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(gallery);
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

            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                return NotFound();
            }

            return View(faq);
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

            if (id != faq.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingFAQ = await _context.FAQs.FindAsync(id);
                    if (existingFAQ == null)
                    {
                        return NotFound();
                    }

                    faq.UpdatedAt = DateTime.Now;
                    faq.CreatedAt = existingFAQ.CreatedAt;

                    _context.Update(faq);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "SSS öğesi başarıyla güncellendi.";
                    return RedirectToAction(nameof(FAQ));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FAQExists(faq.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "SSS öğesi güncellenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(faq);
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

            // Video listesi için placeholder - gerçek uygulamada Video modeli olmalı
            ViewBag.Videos = new List<object>(); // Placeholder
            return View();
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
        public async Task<IActionResult> AddVideoPost()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["Success"] = "Video başarıyla eklendi.";
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
                .GroupBy(p => p.CategoryId)
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

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.Categories.FindAsync(category.Id);
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    // Update existing entity properties instead of using _context.Update()
                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.DisplayOrder = category.DisplayOrder;
                    existingCategory.IsActive = category.IsActive;
                    existingCategory.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Kategori başarıyla güncellendi.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Kategori güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                TempData["Error"] = "Kategori güncellenirken bir hata oluştu.";
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

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = status;
                
                if (status == "Shipped")
                {
                    order.ShippedDate = DateTime.Now;
                }
                else if (status == "Delivered")
                {
                    order.DeliveredDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Sipariş durumu güncellendi.";
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

            var message = await _context.ContactMessages.FindAsync(messageId);
            if (message != null)
            {
                message.IsReplied = true;
                message.RepliedAt = DateTime.Now;
                message.ReplyMessage = replyMessage;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Yanıt gönderildi.";
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

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(user.Id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Sadece belirli alanları güncelle
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.Phone = user.Phone;
                    existingUser.IsAdmin = user.IsAdmin;
                    existingUser.IsActive = user.IsActive;
                    existingUser.UpdatedAt = DateTime.Now;

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Kullanıcı güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                TempData["Error"] = "Kullanıcı güncellenirken bir hata oluştu.";
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

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                try
                {
                    user.IsActive = isActive;
                    user.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Kullanıcı başarıyla {(isActive ? "aktif" : "pasif")} yapıldı.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Kullanıcı durumu güncellenirken bir hata oluştu: " + ex.Message;
                }
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
    }
} 