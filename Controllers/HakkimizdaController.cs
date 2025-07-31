using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 