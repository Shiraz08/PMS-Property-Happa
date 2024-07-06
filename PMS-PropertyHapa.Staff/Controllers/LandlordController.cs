using Microsoft.AspNetCore.Authentication;
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
        public LandlordController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
        }
        public async Task<IActionResult> Index()
        {
            var owner = await _authService.GetAllLandlordAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                owner = owner.Where(s => s.AddedBy == currenUserId);
            }
            return View(owner);
        }
        public IActionResult AddLandlord()
        {
            return View();
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

        //[HttpGet]
        //public async Task<IActionResult> GetLandlordDataById(int ownerId)
        //{
        //    List<LandlordDataDto> owners = await _authService.GetLandlordDataByIdAsync(ownerId);
        //    if (owners == null || owners.Count == 0)
        //    {
        //        return StatusCode(404, "Owner not found");
        //    }
        //    return Ok(owners);
        //}

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


        [HttpDelete]
        public async Task<IActionResult> Delete(string ownerId)
        {
            await _authService.DeleteLandlordAsync(ownerId);
            return Json(new { success = true, message = "Owner deleted successfully" });
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

    }
}
