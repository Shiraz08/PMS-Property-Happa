using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PMS_PropertyHapa.Models.DTO
{
    public class OTPDto
    {
        public string AppTenantId { get; set; }
        public string Type { get; set; }

        public string Code { get; set; }

        public bool PhoneNumber { get; set; }

        public bool Email { get; set; }

        public DateTime Expiry { get; set; }
    }
}
