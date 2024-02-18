using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
{
    public class ApplicationsController : Controller
    {
        public IActionResult AddApplication()
        {
            return View();
        }
        public IActionResult ViewApplication()
        {
            return View();
        }
    }
}
