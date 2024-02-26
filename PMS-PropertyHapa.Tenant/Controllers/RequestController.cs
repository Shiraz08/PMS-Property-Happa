using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Tenant.Controllers
{
    public class RequestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
