using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.API.Controllers
{
    public class teontroller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
