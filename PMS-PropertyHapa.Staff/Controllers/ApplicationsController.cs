using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class ApplicationsController : Controller
    {

        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;

        public ApplicationsController(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService = permissionService;
        }
        [AllowAnonymous]
        public async Task<IActionResult> AddApplication(string id)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddApplication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var terms = await _authService.GetTermsbyId(id);
            ViewBag.Terms = terms;

            return View();
        }
        public async Task<IActionResult> ViewApplication()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewApplication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            ViewBag.UserId = currenUserId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetApplications()
        {
            IEnumerable<ApplicationsDto> applications = new List<ApplicationsDto>();
            applications = await _authService.GetApplicationsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                applications = applications.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(applications);
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SaveApplication(ApplicationsDto applicationsDto)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddApplication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (applicationsDto == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            applicationsDto.AddedBy = Request?.Cookies["userId"]?.ToString();

            await _authService.SaveApplicationAsync(applicationsDto);

            return Json(new { success = true, message = "Applications Data added successfully" });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddApplication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            await _authService.DeleteApplicationAsync(id);
            return Json(new { success = true, message = "Application deleted successfully" });
        }

        public async Task<IActionResult> GetApplicationById(int id)
        {
            ApplicationsDto application = await _authService.GetApplicationByIdAsync(id);
            if (application == null)
            {
                return StatusCode(500, "Application not found");
            }
            return Ok(application);
        }

        public async Task<IActionResult> EditApplication(int id)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddApplication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            ApplicationsDto application = await _authService.GetApplicationByIdAsync(id);
            if (application == null)
            {
                return StatusCode(500, "Application not found");
            }
            return View(application);
        }


        [HttpGet]
        public async Task<IActionResult> GetProperties(string id)
        {
            try
            {
                var properties = await _authService.GetAllAssetsAsync();
                if (id != null)
                {
                    properties = properties.Where(s => s.AddedBy == id);
                }
                return Ok(properties);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching Communications: {ex.Message}");
            }
        }

    }
}
