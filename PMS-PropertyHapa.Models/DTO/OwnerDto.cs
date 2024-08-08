using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PMS_PropertyHapa.Models.DTO
{
    public class OwnerDto
    {
        public int? OwnerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Fax { get; set; }
        public string TaxId { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Document { get; set; }
        public string DocumentName { get; set; }
        public IFormFile DocumentUrl { get; set; }
        public string EmailAddress2 { get; set; }        
        public string PhoneNumber { get; set; }
        public string PhoneNumber2 { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
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
        public string Picture { get; set; }
        public string PictureName { get; set; }
        public IFormFile PictureUrl { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationDescription { get; set; }
        public string OrganizationIcon { get; set; }
        public string OrganizationIconName { get; set; }
        public IFormFile OrganizationIconFile { get; set; }
        public string OrganizationLogo { get; set; }
        public string OrganizationLogoName { get; set; }
        public IFormFile OrganizationLogoFile { get; set; }
        public string Website { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
