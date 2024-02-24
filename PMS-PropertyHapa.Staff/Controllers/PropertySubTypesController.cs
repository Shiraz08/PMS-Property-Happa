using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Staff.Services.IServices;
using PMS_PropertyHapa.Shared.ImageUpload;
using System.Security.Claims;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class PropertySubTypesController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public PropertySubTypesController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }


        [HttpGet]
        public async Task<IActionResult> GetPropertyTypeAll()
        {
            
                var propertyTypes = await _authService.GetAllPropertyTypes();

                if (propertyTypes != null && propertyTypes.Any())
                {
                    return Json(new { data = propertyTypes });
                }
                else
                {
                    return Json(new { data = new List<PropertyTypeDto>() });
                }
            
        }


        public async Task<IActionResult> GetPropertySubTypeData(string tenantId)
        {
            if (!string.IsNullOrEmpty(tenantId))
            {
                var propertyTypes = await _authService.GetPropertySubTypeByIdAllAsync(tenantId);

                if (propertyTypes != null && propertyTypes.Any())
                {
                    return Json(new { data = propertyTypes });
                }
                else
                {
                    return Json(new { data = new List<PropertySubTypeDto>() });
                }
            }
            else
            {
                return BadRequest("Tenant ID is required.");
            }
        }


        public async Task<IActionResult> GetPropertySubType(int propertyTypeId)
        {
            if (propertyTypeId > 0)
            {
                var propertyTypes = await _authService.GetPropertySubTypeByIdAsync(propertyTypeId);

                if (propertyTypes != null && propertyTypes.Any())
                {
                    return Json(new { data = propertyTypes });
                }
                else
                {
                    return Json(new { data = new List<PropertySubTypeDto>() });
                }
            }
            else
            {
                return BadRequest("PropertyType ID is required.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PropertySubTypeDto propertyType)
        {
            propertyType.AppTenantId = Guid.Parse(propertyType.TenantId);

            if (propertyType.Icon_SVG2 != null)
            {
                var (fileName, base64String) = await ImageUploadUtility.UploadImageAsync(propertyType.Icon_SVG2, "uploads");
                propertyType.Icon_String = fileName;
                propertyType.Icon_SVG = base64String;
            }

            propertyType.Icon_SVG2 = null;
            await _authService.CreatePropertySubTypeAsync(propertyType);
            return Json(new { success = true, message = "Property Type added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] PropertySubTypeDto propertyType)
        {
            propertyType.AppTenantId = Guid.Parse(propertyType.TenantId);

            if (propertyType.Icon_SVG2 != null)
            {
                var (fileName, base64String) = await ImageUploadUtility.UploadImageAsync(propertyType.Icon_SVG2, "wwwroot/uploads");
                propertyType.Icon_String = fileName;
                propertyType.Icon_SVG = base64String;
            }
            propertyType.Icon_SVG2 = null;
            await _authService.UpdatePropertySubTypeAsync(propertyType);
            return Json(new { success = true, message = "Property SubType updated successfully" });
        }



        [HttpDelete]
        public async Task<IActionResult> Delete(int propertysubTypeId)
        {
            try
            {
                await _authService.DeletePropertySubTypeAsync(propertysubTypeId);
                return Json(new { success = true, message = "Property SubType deleted successfully" });
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


        public IActionResult AddPropertySubType()
        {
            var model = new PropertySubTypeDto();

            return View(model);
        }

        public async Task<IActionResult> EditPropertySubType(int propertysubTypeId)
        {
            PropertySubTypeDto propertyType = null;


            if (propertysubTypeId > 0)
            {
                propertyType = await _authService.GetSinglePropertySubTypeAsync(propertysubTypeId);

                if (propertyType == null)
                {
                    return NotFound();
                }
            }

            return View("AddPropertySubType", propertyType);
        }



    }
}