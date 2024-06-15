using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class VendorDto
    {
        public int VendorId { get; set; }
        public string FirstName { get; set; }
        public string MI { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public string Notes { get; set; }
        public string Picture { get; set; }
        public string Email1 { get; set; }
        public string Phone1 { get; set; }
        public string Email2 { get; set; }
        public string Phone2 { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string AlterStreet1 { get; set; }
        public string AlterStreet2 { get; set; }
        public string AlterDistrict { get; set; }
        public string AlterCity { get; set; }
        public string AlterState { get; set; }
        public string AlterCountry { get; set; }
        public string Classification { get; set; }
        public string VendorCategoriesIds { get; set; }
        public bool? HasInsurance { get; set; }
        public string InsuranceCompany { get; set; }
        public string PolicyNumber { get; set; }
        public decimal? Amount { get; set; }
        public string TaxId { get; set; }

        public string AccountName { get; set; }
        public string AccountHolder { get; set; }
        public string AccountIBAN { get; set; }
        public string AccountSwift { get; set; }
        public string AccountBank { get; set; }
        public string AccountCurrency { get; set; }

        public string OrganizationName { get; set; }
        public string OrganizationDescription { get; set; }
        public string OrganizationIcon { get; set; }
        public string OrganizationLogo { get; set; }
        public string Website { get; set; }

        public string AddedBy { get; set; }

    }
}
