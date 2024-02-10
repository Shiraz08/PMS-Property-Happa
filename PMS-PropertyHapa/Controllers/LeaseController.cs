using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
{
    public class LeaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddLease()
        {
            return View();
        }
    }
}
