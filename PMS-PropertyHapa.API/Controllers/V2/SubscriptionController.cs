using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.ViewModels;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.API.Controllers.V2
{
    public class SubscriptionController : Controller
    {

        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionController(IUserRepository userRepo, UserManager<ApplicationUser> userManager)
        {
            _userRepo = userRepo;
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
        [Authorize]
        [HttpGet("Subscription")]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            try
            {
                var subscriptions = await _userRepo.GetAllSubscriptionsAsync();

                if (subscriptions != null && subscriptions.Any())
                {
                    return Ok(subscriptions);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "No subscriptions found.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "No subscriptions found",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("Subscription/{Id}")]
        public async Task<IActionResult> GetSubscriptionById(int Id)
        {
            try
            {
                var subscriptionDto = await _userRepo.GetSubscriptionByIdAsync(Id);

                if (subscriptionDto != null)
                {
                    return Ok(subscriptionDto);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"No subscription found with ID: {Id}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"No subscription found with ID: {Id}",
                        Title = "Not Found"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPost("Subscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionDto subscription)
        {
            try
            {
                var isSuccess = await _userRepo.CreateSubscriptionAsync(subscription);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = "Failed to create subscription.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = "Failed to create subscription",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut("Subscription/{Id}")]
        public async Task<IActionResult> UpdateSubscription(int Id, [FromBody] SubscriptionDto subscription)
        {
            try
            {
                subscription.Id = Id; // Ensure subscriptionId is set
                var isSuccess = await _userRepo.UpdateSubscriptionAsync(subscription);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Subscription with ID: {Id} could not be updated.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Subscription with ID: {Id} could not be updated",
                        Title = "Error"
                    }
                }
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("Subscription/{Id}")]
        public async Task<IActionResult> DeleteSubscription(int Id)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteSubscriptionAsync(Id);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    var response = new ApiResponseUser
                    {
                        HasErrors = true,
                        IsValid = false,
                        TextInfo = $"Failed to delete subscription with ID: {Id}.",
                        Result = null,
                        Messages = new[]
                        {
                    new Messages
                    {
                        TypeDescription = MessageType.Error,
                        Message = $"Failed to delete subscription with ID: {Id}",
                        Title = "Error"
                    }
                }
                    };
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        #endregion
    }
}
