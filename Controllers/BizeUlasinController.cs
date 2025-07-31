using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 