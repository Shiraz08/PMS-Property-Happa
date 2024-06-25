using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class TenantOrganizationInfoDto 
    {

        public int Id { get; set; }
        public int TId { get; set; }
        public Guid TenantUserId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationDescription { get; set; }
        public string OrganizationIconName { get; set; }
        public string OrganizationIcon { get; set; }
        public string OrganizationLogoName { get; set; }
        public string OrganizationLogo { get; set; }
        public string OrganizatioPrimaryColor { get; set; }
        public string OrganizationSecondColor { get; set; } 
        public string TempTenantUserId { get; set; } 

    }
}
