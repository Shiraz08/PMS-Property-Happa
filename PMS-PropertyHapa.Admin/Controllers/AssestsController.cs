using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class AssestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddAssest()
        {
            return View();
        }
    }
}
