using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class AccountSubTypeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;

        public AccountSubTypeController(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewAccountSubType);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAccountSubTypes()
        {
            IEnumerable<AccountSubTypeDto> accountSubTypes = new List<AccountSubTypeDto>();
            accountSubTypes = await _authService.GetAccountSubTypesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                accountSubTypes = accountSubTypes.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(accountSubTypes);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAccountSubType([FromBody] AccountSubType accountSubType)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAccountSubType);
            if (!hasAccess)
            {
                return Unauthorized();
            }
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
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAccountSubType);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var response = await _authService.DeleteAccountSubTypeAsync(id);
            if (!response.IsSuccess)
            {
                return Ok(new { success = false, message = string.Join(", ", response.ErrorMessages) });

            }
            return Ok(new { success = true, message = "Sub Account deleted successfully" });

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

        //public async Task<IActionResult> AccountSubTypeDll(int accountTypeId)
        //{
        //    IEnumerable<AccountSubTypeDto> accountSubTypes = new List<AccountSubTypeDto>();
        //    accountSubTypes = await _authService.GetAccountSubTypesAsync();
        //    var currenUserId = Request?.Cookies["userId"]?.ToString();
        //    if (currenUserId != null)
        //    {
        //        accountSubTypes = accountSubTypes.Where(s => s.AddedBy == currenUserId && s.AccountTypeId == accountTypeId);
        //    }
        //    return Ok(accountSubTypes);
        //}

        public async Task<IActionResult> AccountSubTypeDll(Filter filter)
        {
            
            

            var currenUserId = Request?.Cookies["userId"]?.ToString();
            IEnumerable<AccountSubTypeDto> accountSubTypes = new List<AccountSubTypeDto>();
            accountSubTypes = await _authService.GetAccountSubTypesDllAsync(filter);
            //if (currenUserId != null)
            //{
            //    accountSubTypes = accountSubTypes.Where(s => s.AddedBy == currenUserId && s.AccountTypeId == accountTypeId);
            //}
            return Ok(accountSubTypes);
        }

    }
}
