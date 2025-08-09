using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return Content("SİTE ÇALIŞIYOR! ✅ TEST PASSEDi!", "text/html; charset=utf-8");
        }

        public IActionResult Simple()
        {
            ViewBag.Message = "Basit test sayfası çalışıyor!";
            return View();
        }
    }
}
