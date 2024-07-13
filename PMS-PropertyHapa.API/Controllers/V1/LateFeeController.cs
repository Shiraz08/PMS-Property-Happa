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
    [Route("api/v1/LateFeeAuth")]
    [ApiController]
    public class LateFeeController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;
        private readonly GoogleCloudStorageService _storageService;

        public LateFeeController(IUserRepository userRepo, UserManager<ApplicationUser> userManager, GoogleCloudStorageService storageService)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
            _storageService = storageService;
        }

        [HttpPost("GetLateFee")]
        public async Task<ActionResult<LateFeeDto>> GetLateFee(Filter filter)
        {
            try
            {
                var lateFee = await _userRepo.GetLateFeeAsync(filter);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = lateFee;
                return Ok(_response);
    
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add($"An error occurred: {ex.Message}");
                return NotFound(_response);
            }
        }

        [HttpGet("GetLateFeeByAsset/{assetId}")]
        public async Task<ActionResult<LateFeeAssetDto>> GetLateFeeByAsset(int assetId)
        {
            try
            {
                var lateFeeAsset = await _userRepo.GetLateFeeByAssetAsync(assetId);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = lateFeeAsset;
                return Ok(_response);
 
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add($"An error occurred: {ex.Message}");
                return NotFound(_response);
            }
        }

        [HttpPost("LateFee")]
        public async Task<ActionResult<bool>> SaveLateFee(LateFeeDto lateFee)
        {
            try
            {
                var isSuccess = await _userRepo.SaveLateFeeAsync(lateFee);
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
        
        [HttpPost("LateFeeAsset")]
        public async Task<ActionResult<bool>> SaveLateFeeAsset(LateFeeAssetDto lateFeeAsset)
        {
            try
            {
                var isSuccess = await _userRepo.SaveLateFeeAssetAsync(lateFeeAsset);
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
    }
}
