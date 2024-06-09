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
        public async Task<ActionResult> GetLandlordDataById(int id)
        {
            try
            {
                var landlordData = await _userRepo.GetLandlordDataById(id);

                if (landlordData != null)
                {
                    return Ok(landlordData);
                }
                else
                {
                    return NotFound("No user found with this id.");
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
                var TenantData = await _userRepo.GetTenantDataById(id);

                if (TenantData != null)
                {
                    return Ok(TenantData);
                }
                else
                {
                    return NotFound("No user found with this id.");
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
            var url = await _storageService.UploadImageAsync(file);
            return Ok(url);
        }

    }
}
