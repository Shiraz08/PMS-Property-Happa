using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.ViewModels;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;
using System;

namespace PMS_PropertyHapa.API.Controllers.V2
{
    public class PropertyTypeController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public PropertyTypeController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _userManager = userManager;
        }

        [HttpGet("Error")]
        public async Task<IActionResult> Error()
        {
            throw new FileNotFoundException();
        }

        [HttpGet("ImageError")]
        public async Task<IActionResult> ImageError()
        {
            throw new BadImageFormatException("Fake Image Exception");
        }

        #region PropertyTypeCrud

        [HttpGet("PropertyType")]
        public async Task<ActionResult<PropertyTypeDto>> GetAllPropertyTypes()
        {
            try
            {
                var property = await _userRepo.GetAllPropertyTypesAsync();

                if (property != null && property.Any())
                {
                    return Ok(property);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "No property types found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "No property types found",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("PropertyType/{tenantId}")]
        public async Task<IActionResult> GetPropertyTypeById(string tenantId)
        {
            try
            {
                var propertyTypeDto = await _userRepo.GetPropertyTypeByIdAsync(tenantId);

                if (propertyTypeDto != null)
                {
                    return Ok(propertyTypeDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"No property type found with ID: {tenantId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No property type found with ID: {tenantId}",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return NotFound("Error Occurred");
            }
        }

        [HttpGet("GetSinglePropertyType/{propertytypeId}")]
        public async Task<IActionResult> GetSinglePropertyType(int propertytypeId)
        {
            try
            {
                var propertyTypeDto = await _userRepo.GetSinglePropertyTypeByIdAsync(propertytypeId);

                if (propertyTypeDto != null)
                {
                    return Ok(propertyTypeDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"No property type found with ID: {propertytypeId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No property type found with ID: {propertytypeId}",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return NotFound("Error Occurred");
            }
        }

        [HttpPost("PropertyType")]
        public async Task<ActionResult<bool>> CreatePropertyType(PropertyTypeDto tenant)
        {
            try
            {
                var isSuccess = await _userRepo.CreatePropertyTypeAsync(tenant);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "Failed to create property type.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to create property type",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("PropertyType/{PropertyTypeId}")]
        public async Task<ActionResult<bool>> UpdatePropertyType(int PropertyTypeId, PropertyTypeDto tenant)
        {
            try
            {
                tenant.PropertyTypeId = PropertyTypeId; // Ensure tenantId is set
                var isSuccess = await _userRepo.UpdatePropertyTypeAsync(tenant);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Property type with ID: {PropertyTypeId} could not be updated.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Property type with ID: {PropertyTypeId} could not be updated",
                        Title = "Error"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("PropertyType/{propertytypeId}")]
        public async Task<ActionResult<bool>> DeletePropertyType(int propertytypeId)
        {
            try
            {
                var isSuccess = await _userRepo.DeletePropertyTypeAsync(propertytypeId);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Failed to delete property type with ID: {propertytypeId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Failed to delete property type with ID: {propertytypeId}",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        #endregion
    }
}
