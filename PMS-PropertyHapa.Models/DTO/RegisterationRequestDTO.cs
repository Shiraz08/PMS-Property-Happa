namespace PMS_PropertyHapa.Models.DTO
{
    public class RegisterationRequestDTO
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int? TenantId { get; set; }

        public string? SubscriptionName { get; set; }

        public bool EmailConfirmed { get; set; }
    }
}
