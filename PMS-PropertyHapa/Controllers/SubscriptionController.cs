using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Enum;
using PMS_PropertyHapa.Shared.ImageUpload;
using PMS_PropertyHapa.Services.IServices;
using System.Text.Encodings.Web;
using PMS_PropertyHapa.Shared.Email;
using System.Net;
using System.Text;
using System.Web;
using Humanizer;
using PMS_PropertyHapa.Models.Configrations;
using PMS_PropertyHapa.Models.Stripe;
using static PMS_PropertyHapa.Shared.Enum.SD;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using PMS_PropertyHapa.API.Services;
using Stripe;

namespace PMS_PropertyHapa.Controllers
{
    //[Authorize]
    public class SubscriptionController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        EmailSender _emailSender = new EmailSender();
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private readonly IStripeService _stripeService;

        public SubscriptionController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore, IConfiguration configuration, IStripeService stripeService)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
            _configuration = configuration;
            _stripeService = stripeService;
        }

        [HttpGet]
        public IActionResult AddSubscription()
        {
            return View(new SubscriptionDto());
        }

        // POST: Subscriptions/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data", errors = ModelState });
            }

            try
            {
                var sub = await _authService.CreateSubscriptionAsync(dto);

                if (sub)
                {
                    return Json(new { success = true, message = "Subscription added successfully" });

                }

                else
                {
                    return Json(new { success = false, message = "Subscription Failed" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding subscription: " + ex.Message });
            }
        }

        // PUT: Subscriptions/Edit/5
        [HttpPut]
        public async Task<IActionResult> Edit(int id, [FromBody] SubscriptionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data", errors = ModelState });
            }

            var subscription = await _authService.GetSubscriptionsByIdAsync(id);
            if (subscription == null)
            {
                return Json(new { success = false, message = "Subscription not found" });
            }

            try
            {
                var sub = await _authService.UpdateSubscriptionAsync(dto);

                if (sub)
                {
                    return Json(new { success = true, message = "Subscription updated successfully" });

                }

                else
                {
                    return Json(new { success = false, message = "Subscription Update Failed" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating subscription: " + ex.Message });
            }
        }

        // DELETE: Subscriptions/Delete/5
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {

            try
            {
                var sub = await _authService.DeleteSubscriptionAsync(id);

                if (sub)
                {
                    return Json(new { success = true, message = "Subscription Deleted successfully" });

                }

                else
                {
                    return Json(new { success = false, message = "Subscription Delete Failed" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting subscription: " + ex.Message });
            }
        }


        public async Task<IActionResult> GetAllSubscriptionBlocks()
        {
            try
            {
                var subscriptions = await _authService.GetAllSubscriptionBlocksAsync();
                var sortedItems = subscriptions.OrderBy(item => item.SubscriptionType == "Yearly").ToList();

                if (subscriptions == null || !subscriptions.Any())
                {
                    return Json(new { success = false, message = "No subscriptions found." });
                }

                return Json(new { success = true, data = sortedItems });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve subscriptions: {Exception}", ex);
                return Json(new { success = false, message = ex.Message });
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
                var res = await _authService.SavePaymentInformation(paymentInformationDto);

            }
            else if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
            {
                var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                StripeConfiguration.ApiKey = _stripeSettings["SecretKey"];
                var service = new SubscriptionService();
                var sub = service.Get(subscription.Id);
                var paymentservice = new PaymentMethodService();
                var pay = paymentservice.Get(sub.DefaultPaymentMethodId);
                //var Sessionservice = new SessionService();
                //var session = Sessionservice.Get(sessionId);
                var customerService = new CustomerService();
                var customerId = subscription.CustomerId;
                var customer = customerService.Get(customerId);
                var tenantIdString = subscription.Metadata.ContainsKey("tenantId") ? subscription.Metadata["tenantId"] : null;
                var tenantId = string.IsNullOrEmpty(tenantIdString) ? 0 : int.Parse(tenantIdString);
                var UserId = subscription.Metadata.ContainsKey("UserId") ? subscription.Metadata["UserId"] : null;
                //var UserId = string.IsNullOrEmpty(UserIdString) ? 0 : int.Parse(UserIdString);
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

                var result = await _authService.SavePaymentMethodInformation(paymentMethodInformationDto);
                var result2 = await _authService.SaveStripeSubscription(subscriptionDto);
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