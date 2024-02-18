


using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Services.IServices;
using System.Security.Claims;

namespace PMS_PropertyHapa.Controllers
{
    public class PropertyTypesController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public PropertyTypesController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }



        public async Task<IActionResult> GetPropertyType(string tenantId)
        {
            if (!string.IsNullOrEmpty(tenantId))
            {
                var propertyTypes = await _authService.GetPropertyTypeByIdAsync(tenantId);

                if (propertyTypes != null && propertyTypes.Any())
                {
                    return Json(new { data = propertyTypes });
                }
                else
                {
                    return Json(new { data = new List<PropertyTypeDto>() });
                }
            }
            else
            {
                return BadRequest("Tenant ID is required.");
            }
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PropertyTypeDto propertyType)
        {
            propertyType.AppTenantId = Guid.Parse(propertyType.TenantId);
            await _authService.CreatePropertyTypeAsync(propertyType);
            return Json(new { success = true, message = "Property Type added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] PropertyTypeDto propertyType)
        {
            propertyType.AppTenantId = Guid.Parse(propertyType.TenantId);
            await _authService.UpdatePropertyTypeAsync(propertyType);
            return Json(new { success = true, message = "Property Type updated successfully" });
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int propertyTypeId)
        {
            try
            {
                await _authService.DeletePropertyTypeAsync(propertyTypeId);
                return Json(new { success = true, message = "Property Type deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }


        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddPropertyType()
        {
            var model = new PropertyTypeDto();

            return View(model);
        }

        public async Task<IActionResult> EditPropertyType(int propertyTypeId)
        {
            PropertyTypeDto propertyType = null;


            if (propertyTypeId > 0)
            {
                propertyType = await _authService.GetSinglePropertyTypeAsync(propertyTypeId);

                if (propertyType == null)
                {
                    return NotFound();
                }
            }

            return View("AddPropertyType", propertyType);
        }



    }
}