using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class CustomerData
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EmailAddress { get; set; }
        public bool IsCanceled { get; set; }
        public string BillingInterval { get; set; }
        public string SubscriptionType { get; set; }
        public bool? IsTrial { get; set; }
        public string GUID { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public string CustomerId { get; set; }
        public bool HasAdminPermission { get; set; }

        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        public string? SubscriptionName { get; set; }
        public string CompanyName { get; set; }
        
        public int? Expiring {  get; set; }
        public int OwnerCount {  get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; } 
    }
}
