using Microsoft.AspNetCore.Identity;

namespace PMS_PropertyHapa.Admin.Data
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
        public bool Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

    }
}
