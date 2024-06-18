using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class AccountTypeController : Controller
    {
        private readonly IAuthService _authService;

        public AccountTypeController(IAuthService authService)
        {
            _authService = authService;
        }


        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountTypes()
        {
            IEnumerable<AccountType> accountTypes = new List<AccountType>();
            accountTypes = await _authService.GetAccountTypesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                accountTypes = accountTypes.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(accountTypes);
        }


        [HttpPost]
        public async Task<IActionResult> SaveAccountType([FromBody] AccountType accountType)
        {
            if (accountType == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            accountType.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveAccountTypeAsync(accountType);
            return Json(new { success = true, message = "Account added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccountType(int id)
        {
            await _authService.DeleteAccountTypeAsync(id);
            return Json(new { success = true, message = "Account deleted successfully" });
        }

        public async Task<IActionResult> GetAccountTypeById(int id)
        {
            AccountType accountType = await _authService.GetAccountTypeByIdAsync(id);
            if (accountType == null)
            {
                return StatusCode(500, "Account request not found");
            }
            return Ok(accountType);
        }

        public async Task<IActionResult> AccountTypeDll()
        {
            IEnumerable<AccountType> accountTypes = new List<AccountType>();
            accountTypes = await _authService.GetAccountTypesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                accountTypes = accountTypes.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(accountTypes);
        }
    }
}
