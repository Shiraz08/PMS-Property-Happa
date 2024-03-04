using System.ComponentModel.DataAnnotations;

namespace PMS_PropertyHapa.Models.DTO
{
    public class OwnerDto
    {
        public int OwnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Picture { get; set; }
        public string PhoneNumber { get; set; }
        public string EmergencyContactInfo { get; set; }
        public int? LeaseAgreementId { get; set; }

        public string OwnerNationality { get; set; }


        public string Gender { get; set; }

        public string DOB { get; set; }
        public string VAT { get; set; }

        public string LegalName { get; set; }

        public string Account_Name { get; set; }

        public string Account_Holder { get; set; }

        public string Account_IBAN { get; set; }

        public string Account_Swift { get; set; }

        public string Account_Bank { get; set; }

        public string Account_Currency { get; set; }
    }
}
