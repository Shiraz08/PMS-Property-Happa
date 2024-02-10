using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
{
    public class DocumentsController : Controller
    {
        public IActionResult SearchDocuments()
        {
            return View();
        }
    }
}
