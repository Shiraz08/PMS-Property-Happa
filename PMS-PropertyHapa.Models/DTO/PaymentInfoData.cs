using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class PaymentInfoData
    {
        public decimal? Amount {  get; set; }
        public string Payee { get; set; }
        public string SubscriptionType { get; set; }
        public string BillingInterval { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
