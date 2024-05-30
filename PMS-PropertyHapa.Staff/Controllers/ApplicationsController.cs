using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class ApplicationsController : Controller
    {

        private readonly IAuthService _authService;

        public ApplicationsController(IAuthService authService)
        {
            _authService = authService;
        }
        [AllowAnonymous]
        public IActionResult AddApplication()
        {
            return View();
        }
        public async Task<IActionResult> ViewApplication()
        {
            IEnumerable<ApplicationsDto> applications = new List<ApplicationsDto>();
            applications = await _authService.GetApplicationsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                applications = applications.Where(s => s.AddedBy == currenUserId);
            }
            return View(applications);
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SaveApplication(ApplicationsDto applicationsDto)
        {
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
            ApplicationsDto application = await _authService.GetApplicationByIdAsync(id);
            if (application == null)
            {
                return StatusCode(500, "Application not found");
            }
            return View(application);
        }

    }
}
