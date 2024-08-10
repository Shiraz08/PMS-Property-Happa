using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class AccountTypeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;
        public AccountTypeController(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService = permissionService;
        }


        public async Task<IActionResult> Index()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewAccountType);
            if (!hasAccess)
            {
                return Unauthorized();
            }
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
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAccountType);
            if (!hasAccess)
            {
                return Unauthorized();
            }
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
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAccountType);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var response = await _authService.DeleteAccountTypeAsync(id);
            if (!response.IsSuccess)
            {
                return Ok(new { success = false, message = string.Join(", ", response.ErrorMessages) });

            }
            return Ok(new { success = true, message = "Account deleted successfully" });

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
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            var filter = new Filter();
            filter.AddedBy = currenUserId;
            IEnumerable<AccountType> accountTypes = new List<AccountType>();
            accountTypes = await _authService.GetAccountTypesDllAsync(filter);
            //if (currenUserId != null)
            //{
            //    accountTypes = accountTypes.Where(s => s.AddedBy == currenUserId);
            //}
            return Ok(accountTypes);
        }
    }
}
