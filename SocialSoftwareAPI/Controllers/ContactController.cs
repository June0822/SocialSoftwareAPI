using Microsoft.AspNetCore.Mvc;

namespace SocialSoftwareAPI.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
