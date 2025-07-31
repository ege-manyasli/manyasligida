using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace manyasligida.Controllers
{
    public class AdminController : Controller
    {
        private const string AdminSessionKey = "IsAdmin";
        private const string AdminPassword = "admin123"; // Şifreyi buradan değiştirebilirsin

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString(AdminSessionKey) == "true")
                return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public IActionResult Login(string password)
        {
            if (password == AdminPassword)
            {
                HttpContext.Session.SetString(AdminSessionKey, "true");
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Şifre yanlış!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AdminSessionKey);
            return RedirectToAction("Login");
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString(AdminSessionKey) != "true")
                return RedirectToAction("Login");
            return View();
        }

        // Ürün Yönetimi
        public IActionResult Urunler()
        {
            if (HttpContext.Session.GetString(AdminSessionKey) != "true")
                return RedirectToAction("Login");
            return View();
        }
        public IActionResult UrunEkle() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult UrunGuncelle(int id) { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult UrunSil(int id) { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }

        // Galeri Yönetimi
        public IActionResult Galeri() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult GaleriEkle() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult GaleriSil(int id) { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }

        // Video Yönetimi
        public IActionResult Videolar() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult VideoEkle() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult VideoSil(int id) { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }

        // Blog Yönetimi
        public IActionResult Blog() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult BlogEkle() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult BlogGuncelle(int id) { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult BlogSil(int id) { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }

        // SSS Yönetimi
        public IActionResult SSS() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult SSSEkle() { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult SSSGuncelle(int id) { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
        public IActionResult SSSSil(int id) { if (HttpContext.Session.GetString(AdminSessionKey) != "true") return RedirectToAction("Login"); return View(); }
    }
} 