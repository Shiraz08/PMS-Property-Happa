using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{

    public class Subscription
    {
        [Key]
        public int? Id { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionType { get; set; }
        public decimal Price { get; set; }

        public string Currency { get; set; }
        public string SmallDescription { get; set; }
        public decimal Tax { get; set; }
        public int NoOfUnits { get; set; }

        public string AppTenantId { get; set; }
        public int TenantId { get; set; }



    }

}
