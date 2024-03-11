
using System.ComponentModel.DataAnnotations;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Owner : AddressEntities
    {
        [Key]
        public int OwnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Fax { get; set; }
        public string EmailAddress { get; set; }
        public string EmailAddress2 { get; set; }
        public string Picture { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumber2 { get; set; }
        public string EmergencyContactInfo { get; set; } 
        public int? LeaseAgreementId { get; set; }

        [Display(Name = "Nationality")]
        public string OwnerNationality { get; set; }


        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "DOB")]
        public string DOB { get; set; }
        public string VAT { get; set; }

        [Display(Name = "Legal Name")]
        public string LegalName { get; set; }

        [Display(Name = "Account Name")]
        public string Account_Name { get; set; }

        [Display(Name = "Account Holder")]
        public string Account_Holder { get; set; }

        [Display(Name = "IBAN")]
        public string Account_IBAN { get; set; }

        [Display(Name = "SWIFT/BIC")]
        public string Account_Swift { get; set; }

        [Display(Name = "Country Bank Account")]
        public string Account_Bank { get; set; }

        [Display(Name = "Currency")]
        public string Account_Currency { get; set; }
    }
}
