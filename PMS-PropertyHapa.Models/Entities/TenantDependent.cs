using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class TenantDependent : BaseEntities
    {
        [Key]
        public int TenantDependentId { get; set; }
        public int TenantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string DOB { get; set; }
        public string Relation { get; set; }
        public Tenant Tenant { get; set; }
    }
}
