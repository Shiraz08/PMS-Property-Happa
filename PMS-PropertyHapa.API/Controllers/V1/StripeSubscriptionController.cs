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
using PMS_PropertyHapa.Models.Stripe;
using System.Net;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/StripeSubscriptionAuth")]
    [ApiController]
    public class StripeSubscriptionController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;
        private readonly GoogleCloudStorageService _storageService;

        public StripeSubscriptionController(IUserRepository userRepo, UserManager<ApplicationUser> userManager, GoogleCloudStorageService storageService)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
            _storageService = storageService;
        }

        [HttpPost("SavePaymentGuid")]
        public async Task<ActionResult<bool>> SavePaymentGuid(PaymentGuidDto paymentGuidDto)
        {
            try
            {
                var isSuccess = await _userRepo.SavePaymentGuidAsync(paymentGuidDto);
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
        
        [HttpPost("SavePaymentInformation")]
        public async Task<ActionResult<bool>> SavePaymentInformation(PaymentInformationDto paymentInformationDto)
        {
            try
            {
                var isSuccess = await _userRepo.SavePaymentInformationAsync(paymentInformationDto);
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
        
        [HttpPost("SavePaymentMethodInformation")]
        public async Task<ActionResult<bool>> SavePaymentMethodInformation(PaymentMethodInformationDto paymentMethodInformationDto)
        {
            try
            {
                var isSuccess = await _userRepo.SavePaymentMethodInformationAsync(paymentMethodInformationDto);
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
        [HttpPost("SaveStripeSubscription")]
        public async Task<ActionResult<bool>> SaveStripeSubscription(StripeSubscriptionDto stripeSubscriptionDto)
        {
            try
            {
                var isSuccess = await _userRepo.SaveStripeSubscriptionAsync(stripeSubscriptionDto);
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