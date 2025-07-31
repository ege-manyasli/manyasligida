using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Threading.Tasks;

namespace manyasligida.Controllers
{
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // İstatistikler için veritabanından veri çekebiliriz
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => p.IsActive);
            ViewBag.TotalCategories = await _context.Categories.CountAsync(c => c.IsActive);
            
            return View();
        }
    }
} 