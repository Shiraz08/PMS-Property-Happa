using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Owner.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
