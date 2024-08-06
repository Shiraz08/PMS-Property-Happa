using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class SubscriptionInvoice : BaseEntities
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ToName { get; set; }
        public string ToEmail { get; set; }
        public string File { get; set; }
    }
}
