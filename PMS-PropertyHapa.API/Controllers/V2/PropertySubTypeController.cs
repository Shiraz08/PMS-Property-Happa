using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.ViewModels;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.API.Controllers.V2
{

    public class PropertySubTypeController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public PropertySubTypeController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
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

        #region PropertySubTypeCrud 

        [HttpGet("AllPropertyType")]
        public async Task<ActionResult> GetAllPropertyTypes()
        {
            try
            {
                var propertyTypes = await _userRepo.GetAllPropertyTypes();

                if (propertyTypes != null && propertyTypes.Any())
                {
                    return Ok(propertyTypes);
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

        [HttpGet("PropertySubTypeAll/{tenantId}")]
        public async Task<IActionResult> GetPropertyTypeByIdAll(string tenantId)
        {
            try
            {
                var propertyTypeDto = await _userRepo.GetPropertySubTypeByIdAllAsync(tenantId);

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
                        TextInfo = $"No property subtypes found for tenant with ID: {tenantId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No property subtypes found for tenant with ID: {tenantId}",
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

        [HttpGet("PropertySubType")]
        public async Task<ActionResult<PropertySubTypeDto>> GetAllPropertySubTypes()
        {
            try
            {
                var propertySubTypes = await _userRepo.GetAllPropertySubTypesAsync();

                if (propertySubTypes != null && propertySubTypes.Any())
                {
                    return Ok(propertySubTypes);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "No property subtypes found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "No property subtypes found",
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

        [HttpGet("PropertySubType/{propertytypeId}")]
        public async Task<IActionResult> GetPropertySubTypeById(int propertytypeId)
        {
            try
            {
                var propertySubTypeDto = await _userRepo.GetPropertySubTypeByIdAsync(propertytypeId);

                if (propertySubTypeDto != null)
                {
                    return Ok(propertySubTypeDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Property subtype with ID {propertytypeId} not found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Property subtype with ID {propertytypeId} not found",
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

        [HttpGet("GetSinglePropertySubType/{propertysubtypeId}")]
        public async Task<IActionResult> GetSinglePropertySubType(int PropertySubTypeId)
        {
            try
            {
                var propertySubTypeDto = await _userRepo.GetSinglePropertySubTypeByIdAsync(PropertySubTypeId);

                if (propertySubTypeDto != null)
                {
                    return Ok(propertySubTypeDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Property subtype with ID {PropertySubTypeId} not found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Property subtype with ID {PropertySubTypeId} not found",
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

        [HttpPost("PropertySubType")]
        public async Task<ActionResult<bool>> CreatePropertySubType(PropertySubTypeDto tenant)
        {
            try
            {
                var isSuccess = await _userRepo.CreatePropertySubTypeAsync(tenant);

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
                        TextInfo = "Failed to create property subtype.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to create property subtype",
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

        [HttpPut("PropertySubType/{propertysubtypeId}")]
        public async Task<ActionResult<bool>> UpdatePropertySubType(PropertySubTypeDto tenant)
        {
            try
            {
                var isSuccess = await _userRepo.UpdatePropertySubTypeAsync(tenant);

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
                        TextInfo = $"Property subtype with ID {tenant.PropertySubTypeId} could not be updated.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Property subtype with ID {tenant.PropertySubTypeId} could not be updated",
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

        [HttpDelete("PropertySubType/{propertysubtypeId}")]
        public async Task<ActionResult<bool>> DeletePropertySubType(int PropertySubTypeId)
        {
            try
            {
                var isSuccess = await _userRepo.DeletePropertySubTypeAsync(PropertySubTypeId);

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
                        TextInfo = $"Failed to delete property subtype with ID {PropertySubTypeId}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Failed to delete property subtype with ID {PropertySubTypeId}",
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
