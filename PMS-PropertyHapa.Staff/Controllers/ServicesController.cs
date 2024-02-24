using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult PaymentIntegration()
        {
            return View();
        }
        public IActionResult QuickBookIntegration()
        {
            return View();
        }
    }
}
