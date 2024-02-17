using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class InvoiceController : Controller
    {
        public IActionResult PaidInvoices()
        {
            return View();
        }
        public IActionResult DueInvoices()
        {
            return View();
        }
        public IActionResult UnInvoices()
        {
            return View();
        }
    }
}
