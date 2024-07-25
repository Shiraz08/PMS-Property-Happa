using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Models.Stripe
{
    public class CheckoutData
    {
        public string CustomerEmail { get; set; }
        public ProductModel Product { get; set; }
        public RegisterationRequestDTO RegisterationRequest { get; set; }
        public PaymentGuid PaymentGuid { get; set; }
        public int Quantity { get; set; }
        public string UserId { get; set; }
        public PaymentInterval PaymentInterval { get; set; }
        public int PaymentIntervalCount { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public string SuccessCallbackUrl { get; set; }
        public string CancelCallbackUrl { get; set; }
    }
}
