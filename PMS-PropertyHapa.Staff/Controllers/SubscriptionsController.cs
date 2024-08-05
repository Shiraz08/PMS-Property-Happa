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


        [HttpPost]
        public async Task<IActionResult> SavePayment([FromBody] ProductDto productModel)
        {
            var stripeSettings = _configuration.GetSection("StripeSettings");
            var currentUserId = Request?.Cookies["userId"]?.ToString();
            var currentUser = await _authService.GetProfileAsync(currentUserId);
            Guid newGuid = Guid.NewGuid();

            try
            {
                var product = new ProductModel
                {
                    Id = productModel.Id,
                    Title = productModel.Title,
                    Description = productModel.Description,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRG_TUukNvS-0E486weXLkJDpTubsAcdHdmKw&usqp=CAU",
                    Price = productModel.Price * 100, 
                    Currency = "USD"
                };

                var priceOptions = new PriceCreateOptions
                {
                    UnitAmount = product.Price,
                    Currency = product.Currency,
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = productModel.IsYearly ? "year" : "month" 
                    },
                    ProductData = new PriceProductDataOptions
                    {
                        Name = product.Title
                    }
                };
                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceOptions);

                var customerOptions = new CustomerCreateOptions
                {
                    Email = currentUser.Email,
                    Metadata = new Dictionary<string, string>
                            {
                                { "UserId", currentUser.UserId.ToString() },
                                { "ProductId", product.Id.ToString() },
                                { "ProductTitle", product.Title },
                                { "PaymentGuid", newGuid.ToString() },
                                { "IsTrial", productModel.IsTrial ? "true" : "false" } 
                            }
                };
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);

                var setupIntentOptions = new SetupIntentCreateOptions
                {
                    Customer = customer.Id,
                    PaymentMethodTypes = new List<string> { "card" },
                };
                var setupIntentService = new SetupIntentService();
                var setupIntent = await setupIntentService.CreateAsync(setupIntentOptions);

                PaymentGuidDto paymentGuid = new PaymentGuidDto
                {
                    Guid = newGuid.ToString(),
                    Description = "CheckOut to Dashboard",
                    DateTime = DateTime.Now,
                    SessionId = setupIntent.ClientSecret,
                    UserId = currentUser.UserId,
                };

                await _authService.SavePaymentGuid(paymentGuid);
                var pubKey = stripeSettings["PublicKey"];
                var checkoutOrderResponse = new CheckoutOrderDto
                {
                    ClientSecret = setupIntent.ClientSecret,
                    PubKey = pubKey,
                    CustomerId = customer.Id,
                    PriceId = price.Id,
                };

                return Ok(checkoutOrderResponse);
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine($"Error processing payment: {exp.Message}");
                return StatusCode(500, new { error = "An error occurred while processing your payment." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequest request)
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.AttachAsync(
                    request.PaymentMethodId,
                    new PaymentMethodAttachOptions
                    {
                        Customer = request.CustomerId,
                    }
                );

                var customerService = new CustomerService();
                await customerService.UpdateAsync(
                    request.CustomerId,
                    new CustomerUpdateOptions
                    {
                        InvoiceSettings = new CustomerInvoiceSettingsOptions
                        {
                            DefaultPaymentMethod = paymentMethod.Id,
                        },
                    }
                );

                var customer = await customerService.GetAsync(request.CustomerId);

                var subscriptionOptions = new SubscriptionCreateOptions
                {
                    Customer = request.CustomerId,
                    Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions
                {
                    Price = request.PriceId,
                },
            },
                    Metadata = customer.Metadata,
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                };
                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.CreateAsync(subscriptionOptions);

                var paymentIntent = subscription.LatestInvoice.PaymentIntent;

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

                var result = await _authService.SavePaymentInformation(paymentInformationDto);

                var subscriptionDto = new StripeSubscriptionDto
                {
                    IsCanceled = false,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.CurrentPeriodEnd,
                    EmailAddress = customer.Email,
                    SubscriptionId = subscription.Id,
                    UserId = customer.Metadata.ContainsKey("UserId") ? customer.Metadata["UserId"] : null,
                    BillingInterval = subscription.Items.Data[0].Plan.Interval,
                    SubscriptionType = subscription.Items.Data[0].Price.Type,
                    IsTrial = customer.Metadata.ContainsKey("IsTrial") && bool.Parse(customer.Metadata["IsTrial"]),
                    GUID = customer.Metadata.ContainsKey("PaymentGuid") ? customer.Metadata["PaymentGuid"] : null,
                    Currency = subscription.Currency,
                    Status = subscription.Status,
                    CustomerId = subscription.CustomerId,
                };

                var paymentMethodInformationDto = new PaymentMethodInformationDto
                {
                    Country = paymentMethod.BillingDetails.Address.Country,
                    CardType = paymentMethod.Card.Brand,
                    CardHolderName = paymentMethod.BillingDetails.Name,
                    CardLast4Digit = paymentMethod.Card.Last4,
                    ExpiryMonth = paymentMethod.Card.ExpMonth.ToString(),
                    ExpiryYear = paymentMethod.Card.ExpYear.ToString(),
                    Email = paymentMethod.BillingDetails.Email,
                    GUID = customer.Metadata.ContainsKey("PaymentGuid") ? customer.Metadata["PaymentGuid"] : null,
                    PaymentMethodId = paymentMethod.Id,
                    CustomerId = paymentMethod.CustomerId,
                };

                var paymentMethodResult = await _authService.SavePaymentMethodInformation(paymentMethodInformationDto);
                var subscriptionResult = await _authService.SaveStripeSubscription(subscriptionDto);

                return Ok(subscription);
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine($"Error creating subscription: {exp.Message}");
                return StatusCode(500, new { error = "An error occurred while creating the subscription." });
            }
        }

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
