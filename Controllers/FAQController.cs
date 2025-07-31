using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Threading.Tasks;

namespace manyasligida.Controllers
{
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FAQController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var faqs = await _context.FAQs
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
            
            return View(faqs);
        }
    }
} 