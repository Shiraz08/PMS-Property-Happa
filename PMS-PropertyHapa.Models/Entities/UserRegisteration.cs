using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class UserRegisteration
    {
        [Key]
        public int UserRegisterationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }


        public string OrganizationName { get; set; }


        public string PropertyType { get; set; }


        public string Country { get; set; }

        public string PhoneNumber { get; set; }
        public int Units { get; set; }

        public string SEODropdown { get; set; }
    }
}
