using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Roles
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Group { get; set; }
        public string Email { get; set; }
        public int UserTypeId { get; set; }
        public string IP { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public string PasswordShow { get; set; }
        public string Picture { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Locality { get; set; }
        public string District { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public Guid AppTenantId { get; set; }

        public int TenantId { get; set; }
        public bool Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string TiwiloPhone { get; set; }

        public string? SubscriptionName { get; set; }    
        public int? SubscriptionId { get; set; }

        public string CompanyName { get; set; }
        public int? PropertyTypeId { get; set; }
        public int? Units { get; set; }
        public string LeadGenration { get; set; }
        public string CardholderName { get; set; }
        public string CardNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string CVV { get; set; }
        public string StreetAdress { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public bool EmailConfirmed { get; set; }

    }
}
