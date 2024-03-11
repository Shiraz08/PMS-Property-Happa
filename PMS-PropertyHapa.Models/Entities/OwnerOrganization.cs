using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
   
    public class OwnerOrganization
    {
        [Key]
        public int Id { get; set; }
        public int  OwnerId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationDescription { get; set; }
        public string OrganizationIcon { get; set; }
        public string OrganizationLogo { get; set; }
        public string Website { get; set; }

    }
}
