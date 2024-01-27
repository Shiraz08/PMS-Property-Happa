using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    public class teontroller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
