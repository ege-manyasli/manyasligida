using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public ContactController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Contact/Index
        public IActionResult Index()
        {
            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View();
        }

        // POST: Contact/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ContactMessage model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.CreatedAt = DateTime.Now;
                    model.IsRead = false;
                    model.IsReplied = false;

                    _context.ContactMessages.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["MessageSuccess"] = "Mesajınız başarıyla gönderildi. En kısa sürede size dönüş yapacağız.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Mesaj gönderilirken bir hata oluştu: " + ex.Message);
            }

            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View("Index", model);
        }
    }
} 