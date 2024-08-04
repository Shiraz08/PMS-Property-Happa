using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Stripe
{
    public class SubscriptionRequest
    {
        public string CustomerId { get; set; }
        public string PriceId { get; set; }
        public string PaymentMethodId { get; set; }
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string PaymentGuid { get; set; }
        public bool IsTrial { get; set; }
    }
}
