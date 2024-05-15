using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Vendor : BaseEntities
    {
        public int VendorId { get; set; }
        public string FirstName { get; set; }
        public string MI { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public string Notes { get; set; }
        public string Picture { get; set; }
        public string Eamil1 { get; set; }
        public string Phone1 { get; set; }
        public string Eamil2 { get; set; }
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
        public string Service { get; set; }
        public string VendorCategories { get; set; }
        public string InsuranceCompany { get; set; }
        public string PolicyNumber { get; set; }
        public string PropertyName { get; set; }
        public string PropertyLocation { get; set; }
        public string TaxId { get; set; }
        public string TaxAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentAmount { get; set; }
        

    }
}
