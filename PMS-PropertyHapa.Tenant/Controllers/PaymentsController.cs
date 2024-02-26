using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Tenant.Controllers
{
    public class PaymentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
