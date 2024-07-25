using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Stripe
{
    public class StripeSubscriptionDto
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string SubscriptionId { get; set; }

        public string EmailAddress { get; set; }

        public bool IsCanceled { get; set; }

        public string BillingInterval { get; set; }

        public string SubscriptionType { get; set; }

        public bool? IsTrial { get; set; }

        public string GUID { get; set; }

        public string Status { get; set; }

        public string Currency { get; set; }

        public string CustomerId { get; set; }
        public DateTime? ChargedDate { get; set; }
        public decimal? Amount { get; set; }
        public string? AddedBy { get; set; }
    }
}
