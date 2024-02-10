using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
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
