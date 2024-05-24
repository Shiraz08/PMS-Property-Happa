using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class ApplicationDependent : BaseEntities
    {
        [Key]
        public int DependentId { get; set; }
        public int ApplicationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DOB { get; set; }
        public string Relation { get; set; }
    }
}
