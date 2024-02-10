using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class LandlordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        } 
        public IActionResult AddLandlord()
        {
            return View();
        }
    }
}
