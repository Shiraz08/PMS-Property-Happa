using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class TenantOrganizationInfoDto2
    {
    
        public Guid TenantUserId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationDescription { get; set; }
        public string OrganizationIcon { get; set; }
        public string OrganizationLogo { get; set; }
        public string OrganizatioPrimaryColor { get; set; }
        public string OrganizationSecondColor { get; set; }

        public IFormFile OrganizationIconFile { get; set; }
        public IFormFile OrganizationLogoFile { get; set; }

    }
}
