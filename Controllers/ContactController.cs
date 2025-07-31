using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Threading.Tasks;

namespace manyasligida.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // İletişim bilgileri için veritabanından veri çekebiliriz
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string name, string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                TempData["Error"] = "Lütfen tüm alanları doldurun.";
                return RedirectToAction(nameof(Index));
            }

            // TODO: Mesajı veritabanına kaydet veya e-posta gönder
            TempData["Success"] = "Mesajınız başarıyla gönderildi. En kısa sürede size dönüş yapacağız.";
            
            return RedirectToAction(nameof(Index));
        }
    }
} 