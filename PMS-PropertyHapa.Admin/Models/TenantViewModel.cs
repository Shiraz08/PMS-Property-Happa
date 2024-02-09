using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class TenantViewModel
    {
        public PMS_PropertyHapa.Admin.Data.ApplicationUser User { get; set; }
        public TenantOrganizationInfoDto2 OrganizationInfo { get; set; }
    }
}
