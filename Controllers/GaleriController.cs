using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class GalleryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 