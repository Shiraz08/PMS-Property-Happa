using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public DateTime? InvoiceCreatedDate { get; set; }
        public bool? InvoicePaid { get; set; }
        public decimal RentAmount { get; set; }
        public int? LeaseId { get; set; }
        public string Asset { get; set; }
        public string Unit { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public bool? InvoicePaidToOwner { get; set; }
        public string AddedBy { get; set; }
    }
}
