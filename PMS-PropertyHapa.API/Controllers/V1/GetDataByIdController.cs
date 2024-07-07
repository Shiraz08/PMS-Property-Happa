using Google.Apis.Storage.v1;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.Services;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using System.Net;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/GetDataByIdAuth")]
    [ApiController]
    public class GetDataByIdController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;
        private readonly GoogleCloudStorageService _storageService;

        public GetDataByIdController(IUserRepository userRepo, UserManager<ApplicationUser> userManager , GoogleCloudStorageService storageService)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
            _storageService = storageService;
        }


        [HttpGet("GetLandlordDataById/{id}")]
        public async Task<ActionResult<LandlordDataDto>> GetLandlordDataById(int id)
        {
            try
            {
                var landlordData = await _userRepo.GetLandlordDataById(id);

                if (landlordData != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = landlordData;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No landlord found with this id.");
                    return NotFound(_response);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetTenantDataById/{id}")]
        public async Task<ActionResult> GetTenantDataById(int id)
        {
            try
            {
                var tenantData = await _userRepo.GetTenantDataById(id);

                if (tenantData != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = tenantData;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No tenant found with this id.");
                    return NotFound(_response);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("uploadImage")]
        
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var url = await _storageService.UploadImageAsync(file, file.FileName);
            return Ok(url);
        }

    }
}
