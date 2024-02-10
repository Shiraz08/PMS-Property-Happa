using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
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
