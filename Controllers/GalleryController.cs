using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Threading.Tasks;

namespace manyasligida.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GalleryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var galleries = await _context.Galleries
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ToListAsync();
            
            return View(galleries);
        }
    }
} 