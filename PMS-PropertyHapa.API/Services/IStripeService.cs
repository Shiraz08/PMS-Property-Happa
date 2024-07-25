using PMS_PropertyHapa.Models.Stripe;

namespace PMS_PropertyHapa.API.Services
{
    public interface IStripeService
    {
        /// <summary>
        /// Creates session in the stripe, that will be sent in callback as parameter
        /// </summary>
        /// <param name="checkout"></param>
        /// <returns>Session Id</returns>
        Task<string> CreateSessionAsync(CheckoutData data, bool isTrial);
    }
}
