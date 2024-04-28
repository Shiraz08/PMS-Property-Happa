using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class AdditionalUserData
    {
        [Key]
        public int Id { get; set; }
        public string AppTenantId { get; set; }
        public string OrganizationName { get; set; }
        public string PropertyType { get; set; }
        public int Units { get; set; }
        public string SEODropdown { get; set; }
    }
}
