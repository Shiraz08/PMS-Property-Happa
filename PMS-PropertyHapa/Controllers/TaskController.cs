using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
{
    public class TaskController : Controller
    {
        public IActionResult AddTask()
        {
            return View();
        } 
        public IActionResult ViewTask()
        {
            return View();
        }
    }
}
