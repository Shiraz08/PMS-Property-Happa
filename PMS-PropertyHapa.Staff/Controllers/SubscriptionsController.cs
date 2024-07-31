using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.Configrations;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Stripe;
using PMS_PropertyHapa.Staff.Services;
using PMS_PropertyHapa.Staff.Services.IServices;
using Stripe;
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

        //[HttpPost]
        //public async Task<IActionResult> SavePayment([FromBody] ProductModel productModel)
        //{
        //    var _stripeSettings = _configuration.GetSection("StripeSettings");
        //    var currenUserId = Request?.Cookies["userId"]?.ToString();

        //    var CurrentUser = await _authService.GetProfileAsync(currenUserId);
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
        //            Price = productModel.Price * 100,  //cents to dollar
        //            Currency = "USD"
        //        };

        //        CheckoutData request = new CheckoutData
        //        {
        //            CustomerEmail = CurrentUser.Email,
        //            UserId = CurrentUser.UserId,
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
        //            UserId = CurrentUser.UserId,
        //        };
        //        await _authService.SavePaymentGuid(paymentGuid);
        //        return Ok(CheckouOrderResponse);
        //    }
        //    catch (Exception exp)
        //    {
        //        throw;
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> SavePayment([FromBody] ProductModel productModel)
        //{
        //    var _stripeSettings = _configuration.GetSection("StripeSettings");
        //    var currentUserId = Request?.Cookies["userId"]?.ToString();

        //    var currentUser = await _authService.GetProfileAsync(currentUserId);
        //    Guid newGuid = Guid.NewGuid();

        //    try
        //    {
        //        // Prepare product and user details
        //        var product = new ProductModel
        //        {
        //            Id = productModel.Id,
        //            Title = productModel.Title,
        //            Description = productModel.Description,
        //            ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRG_TUukNvS-0E486weXLkJDpTubsAcdHdmKw&usqp=CAU",
        //            Price = productModel.Price * 100, // Amount in cents
        //            Currency = "USD"
        //        };

        //        // Create a Price object for the subscription
        //        var priceOptions = new PriceCreateOptions
        //        {
        //            UnitAmount = product.Price,
        //            Currency = product.Currency,
        //            Recurring = new PriceRecurringOptions
        //            {
        //                Interval = "month" // or "year" depending on your subscription interval
        //            },
        //            ProductData = new PriceProductDataOptions
        //            {
        //                Name = product.Title
        //                // Description and Images are not valid properties here
        //            }
        //        };
        //        var priceService = new PriceService();
        //        var price = await priceService.CreateAsync(priceOptions);

        //        // Create a customer for the subscription
        //        var customerOptions = new CustomerCreateOptions
        //        {
        //            Email = currentUser.Email,
        //            Metadata = new Dictionary<string, string>
        //    {
        //        { "UserId", currentUser.UserId.ToString() },
        //        { "ProductId", product.Id.ToString() },
        //        { "ProductTitle", product.Title }
        //    }
        //        };
        //        var customerService = new CustomerService();
        //        var customer = await customerService.CreateAsync(customerOptions);

        //        // Create the SetupIntent for subscription
        //        var setupIntentOptions = new SetupIntentCreateOptions
        //        {
        //            Customer = customer.Id,
        //            PaymentMethodTypes = new List<string> { "card" },
        //            Metadata = new Dictionary<string, string>
        //    {
        //        { "UserId", currentUser.UserId.ToString() },
        //        { "ProductId", product.Id.ToString() },
        //        { "ProductTitle", product.Title },
        //        { "PaymentGuid", newGuid.ToString() }
        //    }
        //        };
        //        var setupIntentService = new SetupIntentService();
        //        var setupIntent = await setupIntentService.CreateAsync(setupIntentOptions);

        //        // Save payment information to your database
        //        PaymentGuidDto paymentGuid = new PaymentGuidDto
        //        {
        //            Guid = newGuid.ToString(),
        //            Description = "CheckOut to Dashboard",
        //            DateTime = DateTime.Now,
        //            SessionId = setupIntent.Id, // Save SetupIntent ID
        //            UserId = currentUser.UserId,
        //        };
        //        await _authService.SavePaymentGuid(paymentGuid);

        //        // Return the SetupIntent client secret and public key
        //        var pubKey = _stripeSettings["PublicKey"];
        //        var checkoutOrderResponse = new CheckoutOrderDto
        //        {
        //            ClientSecret = setupIntent.ClientSecret, // Correctly return the client secret
        //            PubKey = pubKey,
        //        };

        //        return Ok(checkoutOrderResponse);
        //    }
        //    catch (Exception exp)
        //    {
        //        // Log the exception (logging code not shown here)
        //        return StatusCode(500, new { error = "An error occurred while processing your payment." });
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> SavePayment([FromBody] ProductModel productModel)
        //{
        //    var stripeSettings = _configuration.GetSection("StripeSettings");
        //    var currentUserId = Request?.Cookies["userId"]?.ToString();

        //    var currentUser = await _authService.GetProfileAsync(currentUserId);
        //    Guid newGuid = Guid.NewGuid();

        //    try
        //    {
        //        // Prepare product and user details
        //        var product = new ProductModel
        //        {
        //            Id = productModel.Id,
        //            Title = productModel.Title,
        //            Description = productModel.Description,
        //            ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRG_TUukNvS-0E486weXLkJDpTubsAcdHdmKw&usqp=CAU",
        //            Price = productModel.Price * 100, // Amount in cents
        //            Currency = "USD"
        //        };

        //        // Create a Price object for the subscription
        //        var priceOptions = new PriceCreateOptions
        //        {
        //            UnitAmount = product.Price,
        //            Currency = product.Currency,
        //            Recurring = new PriceRecurringOptions
        //            {
        //                Interval = "month" // or "year" depending on your subscription interval
        //            },
        //            ProductData = new PriceProductDataOptions
        //            {
        //                Name = product.Title
        //                // Description and Images are not valid properties here
        //            }
        //        };
        //        var priceService = new PriceService();
        //        var price = await priceService.CreateAsync(priceOptions);

        //        // Create a customer for the subscription
        //        var customerOptions = new CustomerCreateOptions
        //        {
        //            Email = currentUser.Email,
        //            Metadata = new Dictionary<string, string>
        //    {
        //        { "UserId", currentUser.UserId.ToString() },
        //        { "ProductId", product.Id.ToString() },
        //        { "ProductTitle", product.Title }
        //    }
        //        };
        //        var customerService = new CustomerService();
        //        var customer = await customerService.CreateAsync(customerOptions);

        //        // Create the SetupIntent for subscription
        //        var setupIntentOptions = new SetupIntentCreateOptions
        //        {
        //            Customer = customer.Id,
        //            PaymentMethodTypes = new List<string> { "card" },
        //            Metadata = new Dictionary<string, string>
        //    {
        //        { "UserId", currentUser.UserId.ToString() },
        //        { "ProductId", product.Id.ToString() },
        //        { "ProductTitle", product.Title },
        //        { "PaymentGuid", newGuid.ToString() }
        //    }
        //        };
        //        var setupIntentService = new SetupIntentService();
        //        var setupIntent = await setupIntentService.CreateAsync(setupIntentOptions);

        //        // Save payment information to your database
        //        PaymentGuidDto paymentGuid = new PaymentGuidDto
        //        {
        //            Guid = newGuid.ToString(),
        //            Description = "CheckOut to Dashboard",
        //            DateTime = DateTime.Now,
        //            SessionId = setupIntent.Id, // Save SetupIntent ID
        //            UserId = currentUser.UserId,
        //        };
        //        await _authService.SavePaymentGuid(paymentGuid);

        //        // Return the SetupIntent client secret and public key
        //        var pubKey = stripeSettings["PublicKey"];
        //        var checkoutOrderResponse = new CheckoutOrderDto
        //        {
        //            ClientSecret = setupIntent.ClientSecret, // Correctly return the client secret
        //            PubKey = pubKey,
        //        };

        //        return Ok(checkoutOrderResponse);
        //    }
        //    catch (Exception exp)
        //    {
        //        // Log the exception (logging code should be added here)
        //        Console.Error.WriteLine($"Error processing payment: {exp.Message}");
        //        return StatusCode(500, new { error = "An error occurred while processing your payment." });
        //    }
        //}




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

        [HttpGet]
        public async Task<IActionResult> CheckTrialDays()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            StripeSubscriptionDto trialDays = new StripeSubscriptionDto();
            trialDays = await _authService.CheckTrialDaysAsync(currenUserId);
            return Ok(trialDays);

        }
    }
}
