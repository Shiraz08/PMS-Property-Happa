using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
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
