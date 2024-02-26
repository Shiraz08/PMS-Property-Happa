using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Tenant.Controllers
{
    public class AnnouncementsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
