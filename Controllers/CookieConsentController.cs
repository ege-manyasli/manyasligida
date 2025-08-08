using Microsoft.AspNetCore.Mvc;

namespace manyasligida.Controllers
{
    public class CookieConsentController : Controller
    {
        [HttpPost]
        public IActionResult SimpleSave([FromBody] object requestData = null)
        {
            try
            {
                // Set session flag to indicate consent
                HttpContext.Session.SetString("CookieConsent", "true");
                
                // Log the preferences if provided
                if (requestData != null)
                {
                    Console.WriteLine($"Cookie preferences: {System.Text.Json.JsonSerializer.Serialize(requestData)}");
                }
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ClearConsent()
        {
            try
            {
                // Clear session flag to show banner again
                HttpContext.Session.Remove("CookieConsent");
                return Json(new { success = true, message = "Cookie tercihleri temizlendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
}
