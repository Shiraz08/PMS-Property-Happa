using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Staff.Controllers
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
