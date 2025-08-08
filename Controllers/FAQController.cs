using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    public class FAQController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CartService _cartService;
        private readonly ISiteSettingsService _siteSettingsService;

        public FAQController(IServiceProvider serviceProvider, CartService cartService, ISiteSettingsService siteSettingsService)
        {
            _serviceProvider = serviceProvider;
            _cartService = cartService;
            _siteSettingsService = siteSettingsService;
        }

        // GET: FAQ/Index - Devre dışı bırakıldı
        public IActionResult Index()
        {
            // SSS sayfası kaldırıldı, ana sayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }
    }
} 