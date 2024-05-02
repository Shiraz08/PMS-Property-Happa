
using System.ComponentModel.DataAnnotations;

namespace PMS_PropertyHapa.Models.Entities
{
    public class OTP
    {
        [Key]
        public int Id { get; set; }

        public string Type { get; set; }

        public string Code { get; set; }    

        public string PhoneNumber { get; set; }

        public string Email { get; set; } 

        public DateTime Expiry { get; set; }
       
    }
}
