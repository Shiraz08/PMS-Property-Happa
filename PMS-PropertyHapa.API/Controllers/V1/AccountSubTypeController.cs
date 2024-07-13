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
    [Route("api/v1/AccountSubTypeAuth")]
    [ApiController]
    public class AccountSubTypeController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;
        private readonly GoogleCloudStorageService _storageService;

        public AccountSubTypeController(IUserRepository userRepo, UserManager<ApplicationUser> userManager, GoogleCloudStorageService storageService)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
            _storageService = storageService;
        }

        [HttpGet("AccountSubTypes")]
        public async Task<ActionResult<AccountSubTypeDto>> GetAccountSubTypes()
        {
            try
            {
                var accountSubTypes = await _userRepo.GetAccountSubTypesAsync();

                if (accountSubTypes != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = accountSubTypes;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No sub account found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("AccountSubTypesDll")]
        public async Task<ActionResult<AccountSubTypeDto>> GetAccountSubTypesDll(Filter filter)
        {
            try
            {
                var accountSubTypes = await _userRepo.GetAccountSubTypesDllAsync(filter);

                if (accountSubTypes != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = accountSubTypes;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No sub account found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetAccountSubTypeById/{id}")]
        public async Task<IActionResult> GetAccountSubTypeById(int id)
        {

            try
            {
                var accountSubType = await _userRepo.GetAccountSubTypeByIdAsync(id);

                if (accountSubType != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = accountSubType;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No sub account found with this id.");
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

        [HttpPost("AccountSubType")]
        public async Task<ActionResult<bool>> SaveAccountSubType(AccountSubType accountSubType)
        {
            try
            {
                var isSuccess = await _userRepo.SaveAccountSubTypeAsync(accountSubType);
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

        [HttpPost("AccountSubType/{id}")]
        public async Task<ActionResult<APIResponse>> DeleteAccountSubTypeRequest(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteAccountSubTypeAsync(id);
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
    }
}