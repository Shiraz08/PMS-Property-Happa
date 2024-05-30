using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class VendorController : Controller
    {
        private readonly IAuthService _authService;
        private readonly RoleManager<IdentityRole> _roleManager;
        EmailSender _emailSender = new EmailSender();

        public VendorController(IAuthService authService, RoleManager<IdentityRole> roleManager)
        {
            _authService = authService;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<VendorDto> vendors = new List<VendorDto>();
            vendors = await _authService.GetVendorsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                vendors = vendors.Where(s => s.AddedBy == currenUserId);
            }
            return View(vendors);
        }
        public async Task<IActionResult> VendorCategories()
        {
            IEnumerable<VendorCategory> vendorCategories = new List<VendorCategory>();
            vendorCategories = await _authService.GetVendorCategoriesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                vendorCategories = vendorCategories.Where(s => s.AddedBy == currenUserId);
            }
            return View(vendorCategories);
        }

        [HttpPost]
        public async Task<IActionResult> SaveVendorCategory([FromBody] VendorCategory vendorCategory)
        {
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

        [HttpPost]
        public async Task<IActionResult> SaveVendor([FromBody] VendorDto vendor)
        {
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

            APIResponse result = await _authService.RegisterAsync<APIResponse>(registrationRequest);
            if (!result.IsSuccess)
            {
                return Json(new { success = false, message = "Failed to register tenant as user." });
            }
            var emailContent = $"Welcome {vendor.FirstName} {vendor.LastName},\n\nThank you for registering. Here are your details:\nUsername: {vendor.Email1}\nPassword: Test@123\nTenant ID: {registrationRequest.TenantId}\n\nThank you!";
            //await _emailSender.SendEmailAsync(vendor.Email1, "Welcome to Our Service!", emailContent);


            await _authService.SaveVendorAsync(vendor);
            return Json(new { success = true, message = "Vendor added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            await _authService.DeleteVendorAsync(id);
            return Json(new { success = true, message = "Vendor deleted successfully" });
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




    }
}
