using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Linq;
using System.Threading.Tasks;

namespace manyasligida.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string searchTerm, string sortBy = "name")
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
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            // Sıralama
            switch (sortBy.ToLower())
            {
                case "price":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "new":
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
                case "popular":
                    query = query.Where(p => p.IsPopular).OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            var products = await query.ToListAsync();
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortBy = sortBy;

            return View(products);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            // Benzer ürünler
            var relatedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            if (product.StockQuantity < quantity)
            {
                return Json(new { success = false, message = "Yeterli stok bulunmuyor." });
            }

            // TODO: Kullanıcı girişi yapıldığında sepet işlemleri eklenecek
            // Şimdilik session kullanarak basit bir sepet sistemi

            return Json(new { success = true, message = "Ürün sepete eklendi." });
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && (p.Name.Contains(q) || p.Description.Contains(q)))
                .ToListAsync();

            ViewBag.SearchTerm = q;
            return View("Index", products);
        }
    }
} 