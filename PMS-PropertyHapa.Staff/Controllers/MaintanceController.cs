using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class MaintanceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddMaintance()
        {
            return View();
        }
    }
}
