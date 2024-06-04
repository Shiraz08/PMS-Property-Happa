using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class AccountSubTypeController : Controller
    {
        private readonly IAuthService _authService;

        public AccountSubTypeController(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<AccountSubTypeDto> accountSubTypes = new List<AccountSubTypeDto>();
            accountSubTypes = await _authService.GetAccountSubTypesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                accountSubTypes = accountSubTypes.Where(s => s.AddedBy == currenUserId);
            }
            return View(accountSubTypes);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAccountSubType([FromBody] AccountSubType accountSubType)
        {
            if (accountSubType == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            accountSubType.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveAccountSubTypeAsync(accountSubType);
            return Json(new { success = true, message = "Sub Account added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccountSubType(int id)
        {
            await _authService.DeleteAccountSubTypeAsync(id);
            return Json(new { success = true, message = "Sub Account deleted successfully" });
        }

        public async Task<IActionResult> GetAccountSubTypeById(int id)
        {
            AccountSubType accountSubType = await _authService.GetAccountSubTypeByIdAsync(id);
            if (accountSubType == null)
            {
                return StatusCode(500, "Sub Account request not found");
            }
            return Ok(accountSubType);
        }

        public async Task<IActionResult> AccountSubTypeDll(int accountTypeId)
        {
            IEnumerable<AccountSubTypeDto> accountSubTypes = new List<AccountSubTypeDto>();
            accountSubTypes = await _authService.GetAccountSubTypesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                accountSubTypes = accountSubTypes.Where(s => s.AddedBy == currenUserId && s.AccountTypeId == accountTypeId);
            }
            return Ok(accountSubTypes);
        }

    }
}
