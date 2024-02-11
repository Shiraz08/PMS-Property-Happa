using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class TenantModelDto 
    {

        public int? TenantId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmergencyContactInfo { get; set; }
        public int? LeaseAgreementId { get; set; }
        public string? TenantNationality { get; set; }
        public string? Gender { get; set; }
        public string? DOB { get; set; }
        public string? VAT { get; set; }
        public string? LegalName { get; set; }
        public string? Account_Name { get; set; }
        public string? Account_Holder { get; set; }
        public string? Account_IBAN { get; set; }
        public string? Account_Swift { get; set; }
        public string? Account_Bank { get; set; }
        public string? Account_Currency { get; set; }
        public Guid AppTenantId { get; set; }

        public string AppTid { get; set; }

        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Locality { get; set; }
        public string District { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }

    }
}
