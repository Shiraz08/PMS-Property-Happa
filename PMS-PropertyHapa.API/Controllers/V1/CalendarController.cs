using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using Twilio.Http;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/CalendarAuth")]
    [ApiController]
    public class CalendarController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly APIResponse _response;

        public CalendarController(IUserRepository userRepo, APIResponse response)
        {
            _userRepo = userRepo;
            _response = response;
        }


        //Calender Start
        //[HttpPost("CalendarEvents")]
        //public async Task<ActionResult<CalendarEvent>> GetCalendarEvents(CalendarFilterModel filter)
        //{
        //    try
        //    {
        //        var events = await _userRepo.GetCalendarEventsAsync(filter);

        //        if (events != null)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = true;
        //            _response.Result = events;
        //            return Ok(_response);
        //        }
        //        else
        //        {
        //            _response.StatusCode = HttpStatusCode.NotFound;
        //            _response.IsSuccess = false;
        //            _response.ErrorMessages.Add("No asset found with this id.");
        //            return NotFound(_response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}

        //[HttpPost("OccupancyOverviewEvents")]
        //public async Task<ActionResult<OccupancyOverviewEvents>> GetOccupancyOverviewEvents(CalendarFilterModel filter)
        //{
        //    try
        //    {
        //        var events = await _userRepo.GetOccupancyOverviewEventsAsync(filter);

        //        if (events != null)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = true;
        //            _response.Result = events;
        //            return Ok(_response);
        //        }
        //        else
        //        {
        //            _response.StatusCode = HttpStatusCode.NotFound;
        //            _response.IsSuccess = false;
        //            _response.ErrorMessages.Add("No asset found with this id.");
        //            return NotFound(_response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}

        //[HttpPost("LeaseData/{id}")]

        //public async Task<ActionResult<LeaseDataDto>> GetLeaseDataByIdAsync(int id)
        //{
        //    try
        //    {
        //        var events = await _userRepo.GetLeaseDataByIdAsync(id);

        //        if (events != null)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = true;
        //            _response.Result = events;
        //            return Ok(_response);
        //        }
        //        else
        //        {
        //            _response.StatusCode = HttpStatusCode.NotFound;
        //            _response.IsSuccess = false;
        //            _response.ErrorMessages.Add("No asset found with this id.");
        //            return NotFound(_response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred: {ex.Message}");
        //    }
        //}

        //Calender End


    }
}
