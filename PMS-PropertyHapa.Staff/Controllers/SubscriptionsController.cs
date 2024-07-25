using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.API.Services;
using PMS_PropertyHapa.Models.Configrations;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Stripe;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.Security.Claims;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class SubscriptionsController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IStripeService _stripeService;

        public SubscriptionsController(IAuthService authService, IConfiguration configuration, IStripeService stripeService)
        {
            _authService = authService;
            _configuration = configuration;
            _stripeService = stripeService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("Subscriptions/Success")]
        public IActionResult Success(string sessionId)
        {
            // Optionally, you can retrieve the session details using the sessionId if needed.
            ViewBag.SessionId = sessionId;
            return View();
        }
        public IActionResult Cancel()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            try
            {
                var subscriptions = await _authService.GetAllSubscriptionsAsync();
                var sortedItems = subscriptions.OrderBy(item => item.SubscriptionType == "Yearly").ToList();

                if (subscriptions == null || !subscriptions.Any())
                {
                    return Json(new { success = false, message = "No subscriptions found." });
                }

                return Json(new { success = true, data = sortedItems });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePayment([FromBody] ProductModel productModel)
        {
            var _stripeSettings = _configuration.GetSection("StripeSettings");
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            var CurrentUser = await _authService.GetProfileAsync(currenUserId);
            Guid newGuid = Guid.NewGuid();
            try
            {
                PaymentGuid paymentGatewaysGuid = new PaymentGuid
                {
                    Guid = newGuid.ToString(),
                };
                ProductModel product = new ProductModel
                {
                    Id = productModel.Id,
                    Title = productModel.Title,
                    Description = productModel.Description,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRG_TUukNvS-0E486weXLkJDpTubsAcdHdmKw&usqp=CAU",
                    Price = productModel.Price * 100,  //cents to dollar
                    Currency = "USD"
                };

                CheckoutData request = new CheckoutData
                {
                    CustomerEmail = CurrentUser.Email,
                    UserId = CurrentUser.UserId,
                    Quantity = 1,
                    Product = product,
                    PaymentGuid = paymentGatewaysGuid,
                    PaymentMode = PaymentMode.Subscription,
                    PaymentInterval = PaymentInterval.Month,
                    PaymentIntervalCount = 1,
                    SuccessCallbackUrl = _stripeSettings["SuccessCallbackUrl"],
                    CancelCallbackUrl = _stripeSettings["CancelCallbackUrl"],
                };
                var sessionId = await _stripeService.CreateSessionAsync(request, false);
                var pubKey = _stripeSettings["PublicKey"];

                var CheckouOrderResponse = new CheckoutOrderDto()
                {
                    SessionId = sessionId,
                    PubKey = pubKey,
                };
                PaymentGuidDto paymentGuid = new PaymentGuidDto
                {
                    Guid = newGuid.ToString(),
                    Description = "CheckOut to Dashboard",
                    DateTime = DateTime.Now,
                    SessionId = sessionId,
                    UserId = CurrentUser.UserId,
                };
                return Ok(CheckouOrderResponse);
            }
            catch (Exception exp)
            {
                throw;
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> SavePayment([FromBody] ProductModel productModel)
        //{
        //    var _stripeSettings = _configuration.GetSection("StripeSettings");

        //    Guid newGuid = Guid.NewGuid();
        //    try
        //    {
        //        PaymentGuid paymentGatewaysGuid = new PaymentGuid
        //        {
        //            Guid = newGuid.ToString(),
        //        };
        //        ProductModel product = new ProductModel
        //        {
        //            Id = productModel.Id,
        //            Title = productModel.Title,
        //            Description = productModel.Description,
        //            ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRG_TUukNvS-0E486weXLkJDpTubsAcdHdmKw&usqp=CAU",
        //            Price = productModel.Price,
        //            Currency = "USD"
        //        };

        //        CheckoutData request = new CheckoutData
        //        {
        //            //CustomerEmail = CurrentUser.Email,
        //            //TenantId = CurrentUser.TenantId,
        //            //UserId = CurrentUser.Id,
        //            Quantity = 1,
        //            Product = product,
        //            PaymentGuid = paymentGatewaysGuid,
        //            PaymentMode = PaymentMode.Subscription,
        //            PaymentInterval = PaymentInterval.Month,
        //            PaymentIntervalCount = 1,
        //            SuccessCallbackUrl = _stripeSettings["SuccessCallbackUrl"],
        //            CancelCallbackUrl = _stripeSettings["CancelCallbackUrl"],
        //        };
        //        var sessionId = await _stripeService.CreateSessionAsync(request, false);
        //        var pubKey = _stripeSettings["PublicKey"];

        //        var CheckouOrderResponse = new CheckoutOrderDto()
        //        {
        //            SessionId = sessionId,
        //            PubKey = pubKey,
        //        };
        //        PaymentGuidDto paymentGuid = new PaymentGuidDto
        //        {
        //            Guid = newGuid.ToString(),
        //            Description = "CheckOut to Dashboard",
        //            DateTime = DateTime.Now,
        //            SessionId = sessionId,
        //            //UserId = CurrentUser.Id,
        //            //TenantId = TenantId,
        //        };
        //        //await _subscriptionQueryDomainService.SavePaymentGuid(paymentGuid);
        //        return Ok(CheckouOrderResponse);
        //    }
        //    catch (Exception exp)
        //    {
        //        throw;
        //    }
        //}

    }
}
