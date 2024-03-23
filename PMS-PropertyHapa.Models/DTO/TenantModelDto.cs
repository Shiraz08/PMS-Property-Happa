using Microsoft.AspNetCore.Http;
using PMS_PropertyHapa.Models.Entities;
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

        public string EmailAddress2 { get; set; }
        public string? PhoneNumber { get; set; }

        public string PhoneNumber2 { get; set; }
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

        public string Unit { get; set; }
        public string Locality { get; set; }
        public string District { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }


        public string MiddleName { get; set; }
        public string Picture { get; set; }
        public IFormFile PictureUrl { get; set; }
        public string Document { get; set; }

        public IFormFile DocumentUrl { get; set; }
        public string EmergencyName { get; set; }
        public string EmergencyEmailAddress { get; set; }
        public string EmergencyRelation { get; set; }
        public string EmergencyDetails { get; set; }

        public List<PetDto> Pets { get; set; } = new List<PetDto>();

        public class PetDto
        {
            public int PetId { get; set; }
            public int TenantId { get; set; }
            public string Picture { get; set; }

            public IFormFile PictureUrl2 { get; set; }
            public string Name { get; set; }
            public string Breed { get; set; }
            public string Type { get; set; }
            public int Quantity { get; set; }
            public string TenantName { get; set; }

            public Guid AppTenantId { get; set; }
            public bool Status { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime AddedDate { get; set; }
            public string AddedBy { get; set; }
            public DateTime ModifiedDate { get; set; }
            public string ModifiedBy { get; set; }
        }

        public List<VehicleDto> Vehicles { get; set; } = new List<VehicleDto>();

        public class VehicleDto
        {
            public int VehicleId { get; set; }
            public int TenantId { get; set; }
            public string Manufacturer { get; set; }
            public string ModelName { get; set; }
            public string ModelVariant { get; set; }
            public string Color { get; set; }
            public string Year { get; set; }
        }

        public List<TenantDependentDto> Dependent { get; set; } = new List<TenantDependentDto>();

        public class TenantDependentDto
        {
            public int TenantDependentId { get; set; }
            public int TenantId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
            public string DOB { get; set; }
            public string Relation { get; set; }
        }

        public List<CoTenantDto> CoTenant { get; set; } = new List<CoTenantDto>();

        public class CoTenantDto
        {
            public int CoTenantId { get; set; }
            public int TenantId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string Unit { get; set; }
            public string District { get; set; }
            public string Region { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
        }
    }
}
