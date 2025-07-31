using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public BlogController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Blog/Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {
            try
            {
                var query = _context.Blogs.Where(b => b.IsActive);
                var totalBlogs = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

                var blogs = await query
                    .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPreviousPage = page > 1;
                ViewBag.HasNextPage = page < totalPages;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();

                return View(blogs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Blog yazıları yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Blog>());
            }
        }

        // GET: Blog/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var blog = await _context.Blogs
                    .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

                if (blog == null)
                {
                    return NotFound();
                }

                // Son blog yazıları
                var recentBlogs = await _context.Blogs
                    .Where(b => b.IsActive && b.Id != id)
                    .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                ViewBag.RecentBlogs = recentBlogs;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();

                return View(blog);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Blog detayı yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 