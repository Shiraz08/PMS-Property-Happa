


using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Services.IServices;
using PMS_PropertyHapa.Shared.ImageUpload;
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
                    if (!string.IsNullOrEmpty(propertyTypes.FirstOrDefault().Icon_SVG))
                    {
                        foreach(var item in propertyTypes)
                        {
                            byte[] imageBytes = await Base64ImageConverter.ConvertFromBase64StringAsync(item.Icon_SVG);
                            item.Icon_SVG2 = ConvertToFormFile(imageBytes, "Icon_SVG2");

                        }
                    }
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

        private FormFile ConvertToFormFile(byte[] fileBytes, string fileName)
        {
            var ms = new MemoryStream(fileBytes);
            return new FormFile(ms, 0, ms.Length, null, fileName);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PropertyTypeDto propertyType)
        {
            propertyType.AppTenantId = Guid.Parse(propertyType.TenantId);

            if (propertyType.Icon_SVG2 != null)
            {
                var (fileName, base64String) = await ImageUploadUtility.UploadImageAsync(propertyType.Icon_SVG2, "uploads");
                propertyType.Icon_String = fileName;
                propertyType.Icon_SVG = base64String;
            }

            propertyType.Icon_SVG2 = null;
            await _authService.CreatePropertyTypeAsync(propertyType);
            return Json(new { success = true, message = "Property Type added successfully" });
        }


        [HttpPost]
        public async Task<IActionResult> Update([FromForm] PropertyTypeDto propertyType, IFormFile iconSVG)
        {
            propertyType.AppTenantId = Guid.Parse(propertyType.TenantId);

            if (iconSVG != null)
            {
                var (fileName, base64String) = await ImageUploadUtility.UploadImageAsync(iconSVG, "uploads");
                propertyType.Icon_String = fileName;
                propertyType.Icon_SVG = base64String;
            }

            propertyType.Icon_SVG2 = null;
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