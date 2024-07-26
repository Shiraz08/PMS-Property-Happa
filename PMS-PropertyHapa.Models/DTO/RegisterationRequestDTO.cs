namespace PMS_PropertyHapa.Models.DTO
{
    public class RegisterationRequestDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int? TenantId { get; set; }

        public string? SubscriptionName { get; set; }
        public int? SubscriptionId { get; set; }

        public bool EmailConfirmed { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string Country { get; set; }
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
        public string Currency { get; set; }
        public long Price { get; set; }
        public bool IsYearly { get; set; }
        public bool IsTrial { get; set; }

    }
}
