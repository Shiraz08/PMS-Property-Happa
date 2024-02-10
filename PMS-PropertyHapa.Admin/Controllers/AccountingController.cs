using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class AccountingController : Controller
    {
        public IActionResult Rent()
        {
            return View();
        }
        public IActionResult AssestExpense()
        {
            return View();
        }
        public IActionResult Deposit()
        {
            return View();
        }
        public IActionResult Billing()
        {
            return View(); 
        }
        public IActionResult Invoices()
        {
            return View();
        }
    }
}
