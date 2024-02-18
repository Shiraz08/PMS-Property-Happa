using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using PMS_PropertyHapa.Shared.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Web;
using System.Security.Claims;
using System.Net.Http.Headers;
using PMS_PropertyHapa.Models.Roles;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/PropertySubTypeauth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class PropertySubTypeController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;

        public PropertySubTypeController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
            _response = new();
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
                    var response = new APIResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        IsSuccess = true,
                        Result = propertyTypes
                    };
                    return Ok(response);
                }
                else
                {
                    // If no property types found, return a NotFound response
                    return NotFound(new APIResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        Result = null
                    });
                }
            }
            catch (Exception ex)
            {
                // Return an error response in case of exceptions
                return StatusCode(500, new APIResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Result = null
                });
            }
        }



        [HttpGet("PropertySubType")]
        public async Task<ActionResult<PropertySubTypeDto>> GetAllPropertySubTypes()
        {
            try
            {
                var property = await _userRepo.GetAllPropertySubTypesAsync();
                return Ok(property);
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
                var PropertySubTypeDto = await _userRepo.GetPropertySubTypeByIdAsync(propertytypeId);

                if (PropertySubTypeDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = PropertySubTypeDto;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No PropertySubType found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occured");
                return NotFound(_response);
            }
        }


        [HttpGet("GetSinglePropertySubType/{propertysubtypeId}")]
        public async Task<IActionResult> GetSinglePropertySubType(int PropertySubTypeId)
        {
            try
            {
                var tenantDto = await _userRepo.GetSinglePropertySubTypeByIdAsync(PropertySubTypeId);

                if (tenantDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = tenantDto;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occured");
                return NotFound(_response);
            }
        }


        [HttpPost("PropertySubType")]
        public async Task<ActionResult<bool>> CreatePropertySubType(PropertySubTypeDto tenant)
        {
            try
            {
                var isSuccess = await _userRepo.CreatePropertySubTypeAsync(tenant);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("PropertySubType/{propertysubtypeId}")]
        public async Task<ActionResult<bool>> UpdatePropertySubType(int tenantId, PropertySubTypeDto tenant)
        {
            try
            {
                tenant.PropertySubTypeId = tenantId; // Ensure tenantId is set
                var isSuccess = await _userRepo.UpdatePropertySubTypeAsync(tenant);
                return Ok(isSuccess);
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
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        #endregion





    }
}