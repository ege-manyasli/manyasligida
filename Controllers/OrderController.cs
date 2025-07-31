using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Threading.Tasks;

namespace manyasligida.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Sepet sayfası
            return View();
        }

        public async Task<IActionResult> Cart()
        {
            // Sepet detayları
            return View();
        }

        public async Task<IActionResult> Checkout()
        {
            // Ödeme sayfası
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            // Sipariş oluşturma
            return RedirectToAction(nameof(Index));
        }
    }
} 