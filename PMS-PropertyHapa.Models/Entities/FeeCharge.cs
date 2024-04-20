using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class FeeCharge
    {
        [Key]
        public int FeeChargeId { get; set; }
        public int LeaseId { get; set; }
      
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime FeeDate { get; set; }
    }

}
