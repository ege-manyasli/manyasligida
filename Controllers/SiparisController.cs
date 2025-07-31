using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 