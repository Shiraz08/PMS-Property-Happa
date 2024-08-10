using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Stripe
{
    public class SubscriptionInvoiceDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string ToName { get; set; }
        public string ToEmail { get; set; }
        public string File { get; set; }
        public string AddedBy { get; set; }
    }
}
