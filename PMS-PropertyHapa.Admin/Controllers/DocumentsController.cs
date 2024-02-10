using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class DocumentsController : Controller
    {
        public IActionResult SearchDocuments()
        {
            return View();
        }
    }
}
