﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Staff.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Enum;
using PMS_PropertyHapa.Shared.ImageUpload;
using PMS_PropertyHapa.Staff.Auth.Controllers;
using PMS_PropertyHapa.Staff.Services.IServices;
using Newtonsoft.Json;
using static PMS_PropertyHapa.Shared.Enum.SD;
using PMS_PropertyHapa.Staff.Filters;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class LandlordController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IWebHostEnvironment _environment;
        private readonly IPermissionService _permissionService;

        public LandlordController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore, IPermissionService permissionService)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
            _permissionService = permissionService;
        }

        //[TypeFilter(typeof(PermissionFilter), Arguments = new object[] { UserPermissions.ViewLandlord })]
        public async Task<IActionResult> Index()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewLandlord);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var owner = await _authService.GetAllLandlordAsync();
            if (currenUserId != null)
            {
                owner = owner.Where(s => s.AddedBy == currenUserId);
            }
            return View(owner);
        }
        public async Task<IActionResult> AddLandlord()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddLandlord);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }

        public class LandlordFilter
        {
            public bool IsDaily { get; set; }
            public bool IsWeekly { get; set; }
            public bool IsMonthly { get; set; }
            public bool IsResidential { get; set; }
            public bool IsCommercial { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLandlords(LandlordFilter landlordFilter)
        {
            try
            {
                var owner = await _authService.GetAllLandlordAsync();
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    owner = owner.Where(s => s.AddedBy == currenUserId);
                }

                if (landlordFilter.IsDaily)
                {
                    owner = owner.Where(s => s.AddedDate == DateTime.UtcNow);
                }
                
                if (landlordFilter.IsWeekly)
                {
                    owner = owner.Where(s => s.AddedDate == DateTime.UtcNow);
                }

                if (landlordFilter.IsMonthly)
                {
                    owner = owner.Where(s => s.AddedDate == DateTime.UtcNow);
                }


                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching assets: {ex.Message}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetLandlord()
        {
            try
            {
                var owner = await _authService.GetAllLandlordAsync();
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    owner = owner.Where(s => s.AddedBy == currenUserId);
                }
                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching assets: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLandlordDataById(int id)
        {
            LandlordDataDto owner = await _authService.GetLandlordDataById(id);
            return Ok(owner);
        }

        [HttpGet]
        public async Task<IActionResult> GetLandlordDll()
        {
            try
            {
                
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                var filter = new Filter();
                filter.AddedBy = currenUserId;
                IEnumerable<OwnerDto> owner = new List<OwnerDto>();
                owner = await _authService.GetAllLandlordDllAsync(filter);
                //if (currenUserId != null)
                //{
                //    owner = owner.Where(s => s.AddedBy == currenUserId);
                //}
                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching assets: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] OwnerDto owner)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddLandlord);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (Guid.TryParse(owner.AppTid, out Guid appTenantId))
            {
                owner.AppTenantId = appTenantId;
            }
            else
            {
                // Handle the case where AppTid is not a valid Guid string
                return Json(new { success = false, message = "Invalid AppTid format" });
            }

            //if (owner.Picture != null || owner.OrganizationLogo != null || owner.OrganizationIcon != null)
            //{
            //    owner.Picture = $"data:image/png;base64,{owner.Picture}";
            //    owner.OrganizationLogo = $"data:image/png;base64,{owner.OrganizationLogo}";
            //    owner.OrganizationIcon = $"data:image/png;base64,{owner.OrganizationIcon}";
            //}
            owner.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.CreateLandlordAsync(owner);
            return Json(new { success = true, message = "Owner added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] OwnerDto owner)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddLandlord);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            owner.AppTenantId = Guid.Parse(owner.AppTid);

            if (owner.PictureUrl != null && owner.PictureUrl.Length > 0)
            {
                var (fileName, base64String) = await ImageUploadUtility.UploadImageAsync(owner.PictureUrl, "uploads");
                owner.Picture = $"data:image/png;base64,{base64String}";
            }

            //if (owner.Picture != null || owner.OrganizationLogo != null || owner.OrganizationIcon != null)
            //{
            //    owner.Picture = $"data:image/png;base64,{owner.Picture}";
            //    owner.OrganizationLogo = $"data:image/png;base64,{owner.OrganizationLogo}";
            //    owner.OrganizationIcon = $"data:image/png;base64,{owner.OrganizationIcon}";
            //}
            owner.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.UpdateLandlordAsync(owner);
            return Json(new { success = true, message = "Owner updated successfully" });
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int ownerId)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddLandlord);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var response = await _authService.DeleteLandlordAsync(ownerId);
            if (!response.IsSuccess)
            {
                return Ok(new { success = false, message = string.Join(", ", response.ErrorMessages) });

            }
            return Ok(new { success = true, message = "Owner deleted successfully" });
        }



        [HttpPost]
        [Route("Landlord/Register")]
        public async Task<IActionResult> Register([FromBody] OwnerDto obj)
        { 

            var registrationRequest = new RegisterationRequestDTO
            {
                UserName = obj.UserName,
                Name = obj.Name,
                Password = obj.Password,
                Email = obj.Email,
                Role = string.IsNullOrEmpty(obj.Role) ? SD.User : obj.Role
            };
            APIResponse result = await _authService.RegisterAsync<APIResponse>(registrationRequest);
            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Registered Successfully" });
            }
            var roleList = new List<SelectListItem>()
                                {
                                    new SelectListItem{Text=SD.Admin, Value=SD.Admin},
                                    new SelectListItem{Text=SD.Customer, Value=SD.Customer},
                                };
            ViewBag.RoleList = roleList;
            return View();
        }


        public async Task<IActionResult> EditLandlord(int ownerId)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddLandlord);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            OwnerDto owner = null;

            if (ownerId > 0)
            {
                owner = await _authService.GetSingleLandlordAsync(ownerId);

                if (owner == null)
                {
                    return NotFound();
                }

            }

            return View("AddLandlord", owner);
        }
        public async Task<IActionResult> Download(string id)
        {
            var sessionData = HttpContext.Session.GetString(id);
            if (string.IsNullOrEmpty(sessionData))
                return Content("File not available");

            try
            {
                var parts = sessionData.Split(new[] { '|' }, 2);
                if (parts.Length < 2)
                    return Content("Invalid file data");

                var base64String = parts[0];
                var fileExtension = parts[1]; 

                byte[] fileBytes = Convert.FromBase64String(base64String);
                var memory = new MemoryStream(fileBytes);
                memory.Position = 0;

                string contentType = GetContentType(fileExtension);

                string fileName = $"downloadedFile{fileExtension}";

                return File(memory, contentType, fileName);
            }
            catch (Exception ex)
            {
                return Content($"Error processing your request: {ex.Message}");
            }
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }


        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
    {
        {".txt", "text/plain"},
        {".pdf", "application/pdf"},
        {".doc", "application/vnd.ms-word"},
        {".docx", "application/vnd.ms-word"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".png", "image/png"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".gif", "image/gif"},
        {".csv", "text/csv"},
        {".bmp", "image/bmp"},
        {".ico", "image/x-icon"},
        {".svg", "image/svg+xml"},
        {".tif", "image/tiff"},
        {".tiff", "image/tiff"},
        {".webp", "image/webp"}
    };
        }

        [HttpPost]
        public async Task<IActionResult> GetLandlordOrganization([FromBody] ReportFilter reportFilter)
        {
            reportFilter.AddedBy = Request?.Cookies["userId"]?.ToString();
            var res = await _authService.GetLandlordOrganization(reportFilter);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetLandlordAsset([FromBody] ReportFilter reportFilter)
        {
            reportFilter.AddedBy = Request?.Cookies["userId"]?.ToString();
            var res = await _authService.GetLandlordAsset(reportFilter);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetLandlordReportByExpense()
        {
            try
            {
                var owner = await _authService.GetLandlordReportByExpenseAsync();
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    owner = owner.Where(s => s.AddedBy == currenUserId);
                }
                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching landlord: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLandlordReportByIncome()
        {
            try
            {
                var owner = await _authService.GetLandlordReportByIncomeAsync();
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    owner = owner.Where(s => s.AddedBy == currenUserId);
                }
                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching landlord: {ex.Message}");
            }
        }


        public async Task<IActionResult> LandlordDetails(int id)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewLandlord);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            ViewBag.landlordId = id;
            return View();
        }

        public async Task<IActionResult> GetLandlordDetailById(int ownerId)
        {
            var landlord = await _authService.GetSingleLandlordAsync(ownerId);
            return Ok(landlord);
        }
    }
}
