using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult RentReports()
        {
            return View();
        }
        public IActionResult LeaseReports()
        {
            return View();
        }
        public IActionResult InvoiceReports()
        {
            return View();
        }
    }
}
