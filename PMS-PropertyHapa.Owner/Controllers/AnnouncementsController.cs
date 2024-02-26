using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Owner.Controllers
{
    public class AnnouncementsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
