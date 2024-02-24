using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class CommunicationController : Controller
    {
        public IActionResult SMS()
        {
            return View();
        }

        public IActionResult Email()
        {
            return View();
        }
    }
}
