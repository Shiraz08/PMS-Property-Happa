using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class PaymentGuid : BaseEntities
    {
        [Key]
        public string Guid { get; set; }
        public string Description { get; set; }
        public DateTime? DateTime { get; set; }
        public string SessionId { get; set; }
        public string UserId { get; set; }
    }
}
