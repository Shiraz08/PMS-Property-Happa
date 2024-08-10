using Microsoft.AspNetCore.Mvc;
using NuGet.ContentModel;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.Collections.Generic;
using System.Security.Policy;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class CalendarController : Controller
    {
        private IWebHostEnvironment _webHostEnvironment;
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;
        public CalendarController(IWebHostEnvironment webHostEnvironment, IAuthService authService, IPermissionService permissionService)
        {
            _webHostEnvironment = webHostEnvironment;
            _authService = authService;
            _permissionService = permissionService;
        }
        public async Task<IActionResult> Index()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewCalendar);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }


        public async Task<IActionResult> OccupancyOverview()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewOccupancyOverview);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetEvents([FromBody] CalendarFilterModel filter)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                filter.UserId = currenUserId;
            }
            var list = await _authService.GetCalendarEventsAsync(filter);
             return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> GetCalendarLink(IFormFile calendarFile)
        {
            if(calendarFile == null)
            {
                return BadRequest("No File received.");
            }

            //var currentUser = await GetCurrentUserAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
            
                string wwwPath = _webHostEnvironment.WebRootPath;
                string path = Path.Combine(wwwPath, "CalendarFiles");

                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, currenUserId + ".ics");

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await calendarFile.CopyToAsync(stream);
                }
                var temp = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.ToString();
                var url = Url.Content($"{HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.ToString()}/CalendarFiles/{currenUserId}.ics");

                return Ok(new {url });
            }
            return Ok(new { success=false, response="User not found" });

        }

        [HttpPost]
        public async Task<IActionResult> GetOccupancyCalendarLink(IFormFile calendarFile)
        {
            if (calendarFile == null)
            {
                return BadRequest("No File received.");
            }

            //var currentUser = await GetCurrentUserAsync();

            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                string wwwPath = _webHostEnvironment.WebRootPath;
                string path = Path.Combine(wwwPath, "OccupancyCalendarFiles");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, currenUserId + ".ics");

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await calendarFile.CopyToAsync(stream);
                }
                var temp = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.ToString();
                var url = Url.Content($"{HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.ToString()}/OccupancyCalendarFiles/{currenUserId}.ics");

                return Ok(new { url });
            }
            return Ok(new { success = false, response = "User not found" });
        }

        [HttpGet]
        public async Task<IActionResult> GetOccupancyOverviewResources() 
        {
            var asset = await _authService.GetAllAssetsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                asset = asset.Where(s => s.AddedBy == currenUserId);
            }
            var list = asset.Select(x => new { Id = x.BuildingNo + " - " + x.BuildingName, Title = x.BuildingName }).ToList();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> GetOccupancyOverviewEvents([FromBody] CalendarFilterModel filter)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                filter.UserId = currenUserId;
            }
            var list = await _authService.GetOccupancyOverviewEventsAsync(filter);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> GetLeaseData(int id)
        {
            var lease = await _authService.GetLeaseDataByIdAsync(id);
            if (lease == null)
            {
                return NotFound();
            };
            return Ok(lease);

        }

    }
}
