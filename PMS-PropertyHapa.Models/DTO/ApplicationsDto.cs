using Microsoft.AspNetCore.Http;
using PMS_PropertyHapa.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ApplicationsDto
    {
        public int ApplicationId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string SSN { get; set; }
        public string ITIN { get; set; }
        public DateTime DOB { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string DriverLicenseState { get; set; }
        public string DriverLicenseNumber { get; set; }
        public string Note { get; set; }
        public string Address { get; set; }
        public string LandlordName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhoneNumber { get; set; }
        public DateTime MoveInDate { get; set; }
        public int? MonthlyPayment { get; set; }
        public string JobType { get; set; }
        public string JobTitle { get; set; }
        public int? AnnualIncome { get; set; }
        public string CompanyName { get; set; }
        public bool? WorkStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorEmail { get; set; }
        public string SupervisorPhoneNumber { get; set; }

        public string EmergencyFirstName { get; set; }
        public string EmergencyLastName { get; set; }
        public string EmergencyEmail { get; set; }
        public string EmergencyPhoneNumber { get; set; }
        public string EmergencyAddress { get; set; }
        public string SourceOfIncome { get; set; }
        public int? SourceAmount { get; set; }
        public string Assets { get; set; }
        public int? AssetAmount { get; set; }
        public bool? IsSmoker { get; set; }
        public bool? IsBankruptcy { get; set; }
        public bool? IsEvicted { get; set; }
        public bool? HasPayRentIssue { get; set; }
        public bool? IsCriminal { get; set; }
        public int PropertyId { get; set; }
        public string UnitIds { get; set; }
        public string LicensePicture { get; set; }
        public string LicensePictureName { get; set; }
        public string StubPicture { get; set; }
        public string StubPictureName { get; set; }
        public bool IsAgree { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }

        public List<ApplicationPetsDto> Pets { get; set; }
        public List<ApplicationVehicles> Vehicles { get; set; }
        public List<ApplicationDependent> Dependent { get; set; }
    }


}
