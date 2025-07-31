using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class KVKKController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Gizlilik()
        {
            return View();
        }
    }
} 