﻿using MagicVilla_VillaAPI.Repository.IRepostiory;
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


        //Application Start

        [HttpGet("Applications")]
        public async Task<ActionResult<ApplicationsDto>> GetApplications()
        {
            try
            {
                var applications = await _userRepo.GetApplicationsAsync();

                if (applications != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = applications;
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

        [HttpGet("GetApplicationById/{id}")]
        public async Task<IActionResult> GetApplicationById(int id)
        {

            try
            {
                var application = await _userRepo.GetApplicationByIdAsync(id);

                if (application != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = application;
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

        [AllowAnonymous]
        [HttpPost("Application")]
        public async Task<ActionResult<bool>> SaveApplication(ApplicationsDto application)
        {
            try
            {
                var isSuccess = await _userRepo.SaveApplicationAsync(application);
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

        [HttpPost("Application/{id}")]
        public async Task<ActionResult<bool>> DeleteApplication(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteApplicationAsync(id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpGet("GetTerms/{id}")]
        public async Task<ActionResult<bool>> GetTermsbyId(string id)
        {
            try
            {
                var terms = await _userRepo.GetTermsbyId(id);
                if (terms != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = terms;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No terms found with this id.");
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

        //Application End


        //Vendor Start

        [HttpGet("Vendors")]
        public async Task<ActionResult<Vendor>> GetVendors()
        {
            try
            {
                var vendors = await _userRepo.GetVendorsAsync();

                if (vendors != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = vendors;
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

        [HttpGet("GetVendorById/{id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {

            try
            {
                var vendor = await _userRepo.GetVendorByIdAsync(id);

                if (vendor != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = vendor;
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

        [HttpPost("Vendor")]
        public async Task<ActionResult<bool>> SaveVendor(VendorDto vendor)
        {
            try
            {
                var isSuccess = await _userRepo.SaveVendorAsync(vendor);
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

        [HttpPost("Vendor/{id}")]
        public async Task<ActionResult<bool>> DeleteVendorRequest(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteVendorAsync(id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Vendor End




        //Vendor Category Start

        [HttpGet("VendorCategories")]
        public async Task<ActionResult<VendorCategory>> GetVendorCategories()
        {
            try
            {
                var vendorCategories = await _userRepo.GetVendorCategoriesAsync();

                if (vendorCategories != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = vendorCategories;
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

        [HttpGet("GetVendorCategoryById/{id}")]
        public async Task<IActionResult> GetVendorCategoryById(int id)
        {

            try
            {
                var vendorCategory = await _userRepo.GetVendorCategoryByIdAsync(id);

                if (vendorCategory != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = vendorCategory;
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

        [HttpPost("VendorCategory")]
        public async Task<ActionResult<bool>> SaveVendorCategory(VendorCategory vendorCategory)
        {
            try
            {
                var isSuccess = await _userRepo.SaveVendorCategoryAsync(vendorCategory);
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

        [HttpPost("VendorCategory/{id}")]
        public async Task<ActionResult<bool>> DeleteVendorCategoryRequest(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteVendorCategoryAsync(id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //Vendor Category End


        #region Landlord Crud 

        [HttpGet("Landlord")]
        public async Task<ActionResult<OwnerDto>> GetAllLandlord()
        {
            try
            {
                var assets = await _userRepo.GetAllLandlordAsync();

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



        //[HttpGet("PropertyType/{tenantId}")]
        //public async Task<IActionResult> GetPropertyTypeById(string tenantId)
        //{
        //    try
        //    {
        //        var propertyTypeDto = await _userRepo.GetPropertyTypeByIdAsync(tenantId);

        //        if (propertyTypeDto != null)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = true;
        //            _response.Result = propertyTypeDto;
        //            return Ok(_response);
        //        }
        //        else
        //        {
        //            _response.StatusCode = HttpStatusCode.NotFound;
        //            _response.IsSuccess = false;
        //            _response.ErrorMessages.Add("No propertyType found with this id.");
        //            return NotFound(_response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.StatusCode = HttpStatusCode.NotFound;
        //        _response.IsSuccess = false;
        //        _response.ErrorMessages.Add("Error Occured");
        //        return NotFound(_response);
        //    }
        //}


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
        public async Task<ActionResult<bool>> CreateOwner(OwnerDto owner)
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

        [HttpPut("Landlord/{OwnerId}")]
        public async Task<ActionResult<bool>> UpdateOwner(int OwnerId, OwnerDto owner)
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

        [HttpDelete("Landlord/{ownerId}")]
        public async Task<ActionResult<bool>> DeleteOwner(string ownerId)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteOwnerAsync(ownerId);
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