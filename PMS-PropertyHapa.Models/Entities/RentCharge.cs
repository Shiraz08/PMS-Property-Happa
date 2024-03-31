using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class RentCharge 
    {
        public int RentChargeId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }

        // Foreign key
        public int LeaseId { get; set; }
        // Navigation property
        public virtual Lease Lease { get; set; }
    }
}
