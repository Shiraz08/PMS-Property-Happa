using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using NuGet.ContentModel;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Stripe;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/SubscriptionAuth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class SubscriptionController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;

        public SubscriptionController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
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

        #region SubscriptionCrud 

        [HttpGet("Subscription")]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            try
            {
                var subscriptions = await _userRepo.GetAllSubscriptionsAsync();
                _response.Result = subscriptions;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("Subscription/{Id}")]
        public async Task<IActionResult> GetSubscriptionById(int Id)
        {
            try
            {
                var subscriptionDto = await _userRepo.GetSubscriptionByIdAsync(Id);

                if (subscriptionDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = subscriptionDto;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No subscription found with this ID.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occurred");
                return NotFound(_response);
            }
        }

        [HttpPost("Subscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionDto subscription)
        {
            try
            {
                var isSuccess = await _userRepo.CreateSubscriptionAsync(subscription);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("Subscription/{Id}")]
        public async Task<IActionResult> UpdateSubscription(int Id, [FromBody] SubscriptionDto subscription)
        {
            try
            {
                subscription.Id = Id; // Ensure subscriptionId is set
                var isSuccess = await _userRepo.UpdateSubscriptionAsync(subscription);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("Subscription/{Id}")]
        public async Task<IActionResult> DeleteSubscription(int Id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteSubscriptionAsync(Id);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("TrialDays/{currenUserId}")]
        public async Task<ActionResult<Models.Stripe.StripeSubscriptionDto>> CheckTrialDays(string currenUserId)
        {
            try
            {
                var trialdays = await _userRepo.CheckTrialDaysAsync(currenUserId);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = trialdays;
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