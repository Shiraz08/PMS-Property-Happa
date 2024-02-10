using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class VendorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddVendor()
        {
            return View();
        }
    }
}
