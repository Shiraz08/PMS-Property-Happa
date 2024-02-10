using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
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
