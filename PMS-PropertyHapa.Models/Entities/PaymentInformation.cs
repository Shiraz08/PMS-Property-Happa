using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class PaymentInformation : BaseEntities
    {
        [Key]
        public int Id { get; set; }
        public decimal? ProductPrice { get; set; }
        public decimal? AmountCharged { get; set; }
        public DateTime? ChargeDate { get; set; }
        public string TransactionId { get; set; }
        public string PaymentStatus { get; set; }
        public string Currency { get; set; }
        public string CustomerId { get; set; }
        public int? SelectedSubscriptionId { get; set; }
        public int? NoOfUnits { get; set; }
    }

}
