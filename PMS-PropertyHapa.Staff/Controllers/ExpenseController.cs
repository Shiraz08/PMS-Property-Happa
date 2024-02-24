using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class ExpenseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddExpense()
        {
            return View();
        }
    }
}
