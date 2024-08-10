using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class VendorController : Controller
    {
        private readonly IAuthService _authService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPermissionService _permissionService;
        EmailSender _emailSender = new EmailSender();

        public VendorController(IAuthService authService, RoleManager<IdentityRole> roleManager, IPermissionService permissionService)
        {
            _authService = authService;
            _roleManager = roleManager;
            _permissionService = permissionService;
        }
        public async Task<IActionResult> Index()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewVendor);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }

        public async Task<IActionResult> VendorCategories()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewVendorCategories);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }

        public async Task<IActionResult> VendorClassification()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewVendorClassification);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveVendorCategory([FromBody] VendorCategory vendorCategory)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddVendorCategories);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (vendorCategory == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            vendorCategory.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveVendorCategoryAsync(vendorCategory);
            return Json(new { success = true, message = "Vendor Category added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVendorCategory(int id)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddVendorCategories);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            await _authService.DeleteVendorCategoryAsync(id);
            return Json(new { success = true, message = "Vendor Category deleted successfully" });
        }

        public async Task<IActionResult> GetVendorCategoryById(int id)
        {
            VendorCategory vendorCategory = await _authService.GetVendorCategoryByIdAsync(id);
            if (vendorCategory == null)
            {
                return StatusCode(500, "Vendor Category not found");
            }
            return Ok(vendorCategory);
        }

        [HttpGet]
        public async Task<IActionResult> GetVendorCategories()
        {
            IEnumerable<VendorCategory> vendorCategories = new List<VendorCategory>();
            vendorCategories = await _authService.GetVendorCategoriesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                vendorCategories = vendorCategories.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(vendorCategories);
        }
        public async Task<IActionResult> GetVendorCategoriesDll()
        {
            
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            var filter = new Filter();
            filter.AddedBy = currenUserId;
            IEnumerable<VendorCategory> vendorCategories = new List<VendorCategory>();
            vendorCategories = await _authService.GetVendorCategoriesDllAsync(filter);
            //if (currenUserId != null)
            //{
            //    vendorCategories = vendorCategories.Where(s => s.AddedBy == currenUserId);
            //}
            return Ok(vendorCategories);
        }


        [HttpPost]
        public async Task<IActionResult> SaveVendorClassification([FromBody] VendorClassification vendorClassification)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddVendorClassification);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (vendorClassification == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            vendorClassification.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveVendorClassificationAsync(vendorClassification);
            return Json(new { success = true, message = "Vendor Classification added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVendorClassification(int id)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddVendorClassification);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            await _authService.DeleteVendorClassificationAsync(id);
            return Json(new { success = true, message = "Vendor Classification deleted successfully" });
        }

        public async Task<IActionResult> GetVendorClassificationById(int id)
        {
            VendorClassification vendorClassification = await _authService.GetVendorClassificationByIdAsync(id);
            if (vendorClassification == null)
            {
                return StatusCode(500, "Vendor Classification not found");
            }
            return Ok(vendorClassification);
        }

        [HttpGet]
        public async Task<IActionResult> GetVendorClassifications()
        {
            IEnumerable<VendorClassification> vendorClassifications = new List<VendorClassification>();
            vendorClassifications = await _authService.GetVendorClassificationsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                vendorClassifications = vendorClassifications.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(vendorClassifications);
        }

        public async Task<IActionResult> GetVendorClassificationsDll()
        {
           
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            var filter = new Filter();
            filter.AddedBy = currenUserId;
            IEnumerable<VendorClassification> vendorClassifications = new List<VendorClassification>();
            vendorClassifications = await _authService.GetVendorClassificationsDllAsync(filter);
            //if (currenUserId != null)
            //{
            //    vendorClassifications = vendorClassifications.Where(s => s.AddedBy == currenUserId);
            //}
            return Ok(vendorClassifications);
        }

        [HttpPost]
        public async Task<IActionResult> SaveVendor([FromForm] VendorDto vendor)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddVendor);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (vendor == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            vendor.AddedBy = Request?.Cookies["userId"]?.ToString();

            if (!await _roleManager.RoleExistsAsync("Vendor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Vendor"));
            }
            var registrationRequest = new RegisterationRequestDTO
            {
                UserName = vendor.Email1,
                Name = $"{vendor.FirstName} {vendor.LastName}",
                Email = vendor.Email1,
                Password = "Test@123",
                Role = "Vendor",
            };
            if (vendor.VendorId == 0)
            {
                APIResponse result = await _authService.RegisterAsync<APIResponse>(registrationRequest);
                if (!result.IsSuccess)
                {
                    return Json(new { success = false, message = "Failed to register tenant as user." });
                }
                var emailContent = $"Welcome {vendor.FirstName} {vendor.LastName},\n\nThank you for registering. Here are your details:\nUsername: {vendor.Email1}\nPassword: Test@123\nTenant ID: {registrationRequest.TenantId}\n\nThank you!";
                await _emailSender.SendEmailAsync(vendor.Email1, "Welcome to Our Service!", emailContent);
            }

            await _authService.SaveVendorAsync(vendor);
            return Json(new { success = true, message = "Vendor added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddVendor);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var response = await _authService.DeleteVendorAsync(id);
            if (!response.IsSuccess)
            {
                return Ok(new { success = false, message = string.Join(", ", response.ErrorMessages) });

            }
            return Ok(new { success = true, message = "Owner deleted successfully" });

        }

        public async Task<IActionResult> GetVendorById(int id)
        {
            VendorDto vendor = await _authService.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return StatusCode(500, "Vendor not found");
            }
            return Ok(vendor);
        }
        
        
        public async Task<IActionResult> GetVendors()
        {
            IEnumerable<VendorDto> vendors = new List<VendorDto>();
            vendors = await _authService.GetVendorsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                vendors = vendors.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(vendors);
        }

        public async Task<IActionResult> GetVendorsDll()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            var filter = new Filter();
            filter.AddedBy = currenUserId;
            IEnumerable<VendorDto> vendors = new List<VendorDto>();
            vendors = await _authService.GetVendorsDllAsync(filter);
            //if (currenUserId != null)
            //{
            //    vendors = vendors.Where(s => s.AddedBy == currenUserId);
            //}
            return Ok(vendors);
        }

    }
}
