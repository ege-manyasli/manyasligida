using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 