using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class PaymentsController : Controller
    {
        public IActionResult PaidPayments()
        {
            return View();
        }
        public IActionResult DuePayments()
        {
            return View();
        }
        public IActionResult UnPiadPayments()
        {
            return View();
        }
    }
}
