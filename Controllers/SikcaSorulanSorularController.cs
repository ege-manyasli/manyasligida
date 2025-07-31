using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class FAQController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 