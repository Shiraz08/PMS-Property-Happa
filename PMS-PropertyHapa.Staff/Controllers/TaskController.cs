using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Staff.Controllers
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
