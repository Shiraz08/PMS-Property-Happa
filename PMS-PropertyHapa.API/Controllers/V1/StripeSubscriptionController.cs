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
using Stripe;
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
        private readonly IConfiguration _configuration;


        public StripeSubscriptionController(IUserRepository userRepo, UserManager<ApplicationUser> userManager, GoogleCloudStorageService storageService, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
            _storageService = storageService;
            _configuration = configuration;
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


        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var _stripeSettings = _configuration.GetSection("StripeSettings");

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], _stripeSettings["EndPointKey"]);
            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                var paymentInformationDto = new PaymentInformationDto
                {
                    TransactionId = paymentIntent.Id,
                    ProductPrice = paymentIntent.Amount / 100,
                    AmountCharged = paymentIntent.AmountReceived / 100,
                    ChargeDate = paymentIntent.Created,
                    PaymentStatus = paymentIntent.Status,
                    Currency = paymentIntent.Currency,
                    CustomerId = paymentIntent.CustomerId,
                };
                //Save payment INfo api
                var res = await _userRepo.SavePaymentInformationAsync(paymentInformationDto);

            }
            else if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
            {
                var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                StripeConfiguration.ApiKey = _stripeSettings["SecretKey"];
                var service = new SubscriptionService();
                var sub = service.Get(subscription.Id);
                var paymentservice = new PaymentMethodService();
                var pay = paymentservice.Get(sub.DefaultPaymentMethodId);
                var customerService = new CustomerService();
                var customerId = subscription.CustomerId;
                var customer = customerService.Get(customerId);
                var tenantIdString = subscription.Metadata.ContainsKey("tenantId") ? subscription.Metadata["tenantId"] : null;
                var tenantId = string.IsNullOrEmpty(tenantIdString) ? 0 : int.Parse(tenantIdString);
                var UserId = subscription.Metadata.ContainsKey("UserId") ? subscription.Metadata["UserId"] : null;
                var Guid = subscription.Metadata.ContainsKey("PaymentGuid") ? subscription.Metadata["PaymentGuid"] : null;
                var isTrialString = subscription.Metadata.ContainsKey("IsTrial") ? subscription.Metadata["IsTrial"] : null;
                bool istrial = bool.Parse(isTrialString);
                var subscriptionDto = new StripeSubscriptionDto
                {
                    IsCanceled = false,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.CurrentPeriodEnd,
                    EmailAddress = customer.Email,
                    SubscriptionId = subscription.Id,
                    UserId = UserId,
                    BillingInterval = subscription.Items.Data[0].Plan.Interval,
                    SubscriptionType = subscription.Items.Data[0].Price.Type,
                    IsTrial = istrial,
                    GUID = Guid.ToString(),
                    Currency = subscription.Currency,
                    Status = sub.Status,
                    CustomerId = subscription.CustomerId,
                };
                var paymentMethodInformationDto = new PaymentMethodInformationDto
                {
                    Country = pay.BillingDetails.Address.Country,
                    CardType = pay.Card.Brand,
                    CardHolderName = pay.BillingDetails.Name,
                    CardLast4Digit = pay.Card.Last4,
                    ExpiryMonth = pay.Card.ExpMonth.ToString(),
                    ExpiryYear = pay.Card.ExpYear.ToString(),
                    Email = pay.BillingDetails.Email,
                    GUID = Guid.ToString(),
                    PaymentMethodId = pay.Id,
                    CustomerId = pay.CustomerId,
                };
                //Save payment method api
                //Save payment subscription  api
                var result = await _userRepo.SavePaymentMethodInformationAsync(paymentMethodInformationDto);
                var result2 = await _userRepo.SaveStripeSubscriptionAsync(subscriptionDto);
            }
            else
            {
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                    return BadRequest();
                }
            }
            return Ok();
        }

    }
}