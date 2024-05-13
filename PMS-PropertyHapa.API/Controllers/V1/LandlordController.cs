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






        //Calender Start
        [HttpPost("CalendarEvents")]
        public async Task<ActionResult<CalendarEvent>> GetCalendarEvents(CalendarFilterModel filter)
        {
            try
            {
                var events = await _userRepo.GetCalendarEventsAsync(filter);

                if (events != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = events;
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

        [HttpPost("OccupancyOverviewEvents")]

        public async Task<ActionResult<OccupancyOverviewEvents>> GetOccupancyOverviewEvents(CalendarFilterModel filter)
        {
            try
            {
                var events = await _userRepo.GetOccupancyOverviewEventsAsync(filter);

                if (events != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = events;
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
        
        [HttpPost("LeaseData/{id}")]

        public async Task<ActionResult<LeaseDataDto>> GetLeaseDataByIdAsync(int id)
        {
            try
            {
                var events = await _userRepo.GetLeaseDataByIdAsync(id);

                if (events != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = events;
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

        //Calender End




        #region Landlord Crud 

        [HttpGet("Tasks")]
        public async Task<ActionResult<TaskRequestDto>> GetTasks()
        {
            try
            {
                var assets = await _userRepo.GetTaskRequestsAsync();

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

        [HttpGet("GetTaskById/{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {

            try
            {
                var taskDto = await _userRepo.GetTaskByIdAsync(id);

                if (taskDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = taskDto;
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

        [HttpPost("Task")]
        public async Task<ActionResult<bool>> SaveTaskRequest(TaskRequestDto taskRequestDto)
        {
            try
            {
                var isSuccess = await _userRepo.SaveTaskAsync(taskRequestDto);
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


        [HttpPost("Task/{id}")]
        public async Task<ActionResult<bool>> DeleteTaskRequest(int id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteTaskAsync(id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



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

    }
}