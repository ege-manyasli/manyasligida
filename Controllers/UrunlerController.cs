using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail(int id)
        {
            return View();
        }
    }
} 