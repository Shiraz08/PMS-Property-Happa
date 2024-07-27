using PMS_PropertyHapa.Models.Configrations;
using PMS_PropertyHapa.Models.Stripe;
using Stripe;
using Stripe.Checkout;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Services
{
    public class StripeService : IStripeService

    {
        private readonly IConfiguration _configuration;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        public async Task<string> CreateSessionAsync(CheckoutData data, bool isTrial)
        {
            try
            {
                var _stripeSettings = _configuration.GetSection("StripeSettings");

                StripeConfiguration.ApiKey = _stripeSettings["SecretKey"];
                var priceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = data.Product.Price, // Price is in USD cents.
                    Currency = data.Product.Currency,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = data.Product.Title,
                        Description = data.Product.Description,
                        Images = new List<string> { data.Product.ImageUrl },

                    }
                };

                if (data.PaymentMode == PaymentMode.Subscription)
                {
                    priceData.Recurring = new SessionLineItemPriceDataRecurringOptions()
                    {
                        Interval = data.PaymentInterval.ToString().ToLower(),
                        IntervalCount = data.PaymentIntervalCount

                    };
                }
                var options = new SessionCreateOptions
                {
                    SubscriptionData = new SessionSubscriptionDataOptions
                    {
                        Metadata = new Dictionary<string, string>()
                            {
                                {"UserId",data.UserId.ToString()},
                                {"IsTrial",isTrial.ToString() },
                                {"PaymentGuid",data.PaymentGuid.Guid.ToString()},
                            },
                        TrialPeriodDays = isTrial ? 1 : null,
                    },

                    CustomerEmail = data.CustomerEmail,
                    // Stripe calls the URLs below when certain checkout events happen such as success and failure.
                    SuccessUrl = $"{data.SuccessCallbackUrl}?sessionId=" + "{CHECKOUT_SESSION_ID}", // Customer paid.
                    CancelUrl = data.CancelCallbackUrl,  // Checkout cancelled.
                    PaymentMethodTypes = new List<string> // Only card available in test mode?
                {
                    "card"
                },
                    LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = priceData,
                        Quantity = data.Quantity,
                    },
                },
                    Mode = data.PaymentMode == PaymentMode.OneTime ? "payment" : "subscription"// One-time payment. Stripe supports recurring 'subscription' payments.
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return session.Id;
            }
            catch (Exception exp)
            {

                throw;
            }


        }
    }
}