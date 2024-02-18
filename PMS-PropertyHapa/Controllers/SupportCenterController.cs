using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
{
    public class SupportCenterController : Controller
    {
        public IActionResult AddTickets()
        {
            return View();
        }
        public IActionResult ViewTickets()
        {
            return View();
        }
    }
}
