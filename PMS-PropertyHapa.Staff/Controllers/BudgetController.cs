using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class BudgetController : Controller
    {
        private readonly IAuthService _authService;


        public BudgetController(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<BudgetDto> budgets = new List<BudgetDto>();
            budgets = await _authService.GetBudgetsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                budgets = budgets.Where(s => s.AddedBy == currenUserId);
            }
            return View(budgets);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveBudget(BudgetDto budget, string BudgetItemsJson)
        {

            if (budget == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            budget.ItemsJson = BudgetItemsJson;
            budget.AddedBy = Request?.Cookies["userId"]?.ToString();
           await _authService.SaveBudgetAsync(budget);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DuplicateBudget(int id)
        {
            var budget = await _authService.GetBudgetByIdAsync(id);
            return View(budget);
        }

        public async Task<IActionResult> SaveDuplicateBudget(BudgetDto budget, string BudgetItemsJson)
        {
            if (budget == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            budget.ItemsJson = BudgetItemsJson;
            budget.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveDuplicateBudgetAsync(budget);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteBudget(int id)
        {
            await _authService.DeleteBudgetAsync(id);
            return  RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //public async Task<IActionResult> SaveBudget([FromBody] Budget budget)
        //{
        //    if (budget == null)
        //    {
        //        return Json(new { success = false, message = "Received data is null." });
        //    }
        //    budget.AddedBy = Request?.Cookies["userId"]?.ToString();
        //    await _authService.SaveBudgetAsync(budget);
        //    return Json(new { success = true, message = "Sub Account added successfully" });
        //}




    }
}
