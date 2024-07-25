using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class PaymentGuidDto
    {
        public string Guid { get; set; }

        public string Description { get; set; }

        public DateTime? DateTime { get; set; }

        public string SessionId { get; set; }

        public string? UserId { get; set; }

        public string AddedBy {  get; set; }

    }
}
