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
using PMS_PropertyHapa.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/LandlordAuth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class LandlordController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;

        public LandlordController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
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

        #region Landlord Crud 

        [HttpGet("Landlord")]
        public async Task<ActionResult<OwnerDto>> GetAllLandlord()
        {
            try
            {
                var assets = await _userRepo.GetAllLandlordAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = assets;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("LandlordDll")]
        public async Task<ActionResult<OwnerDto>> GetAllLandlordDll(Filter filter)
        {
            try
            {
                var assets = await _userRepo.GetAllLandlordDllAsync(filter);

                if (assets != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = assets;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No asset found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetSingleLandlord/{ownerId}")]
        public async Task<IActionResult> GetSingleLandlord(int ownerId)
        {
            try
            {
                var tenantDto = await _userRepo.GetSingleLandlordByIdAsync(ownerId);

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


        [HttpPost("Landlord")]
        public async Task<ActionResult<bool>> CreateOwner([FromForm] OwnerDto owner)
        {
            try
            {
                var isSuccess = await _userRepo.CreateOwnerAsync(owner);
                if (isSuccess == true)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = isSuccess;
                }
                return Ok(_response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("Landlord/{OwnerId}")]
        public async Task<ActionResult<bool>> UpdateOwner(int OwnerId, [FromForm] OwnerDto owner)
        {
            try
            {
                owner.OwnerId = OwnerId; // Ensure ownerId is set
                var isSuccess = await _userRepo.UpdateOwnerAsync(owner);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("DeleteLandlord/{ownerId}")]
        public async Task<ActionResult<APIResponse>> DeleteOwner(int ownerId)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteOwnerAsync(ownerId);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return NotFound(_response);
            }
        }


        [HttpPost("LandlordOrganization")]
        public async Task<IActionResult> GetLandlordOrganization(ReportFilter reportFilter)
        {
            try
            {
                var organizations = await _userRepo.GetLandlordOrganizationAsync(reportFilter);
                if (organizations != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = organizations;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
                return Ok(_response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("LandlordAsset")]
        public async Task<IActionResult> GetLandlordAsset(ReportFilter reportFilter)
        {
            try
            {
                var assets = await _userRepo.GetLandlordAssetAsync(reportFilter);
                if (assets != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = assets;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
                return Ok(_response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        #endregion

    }
}