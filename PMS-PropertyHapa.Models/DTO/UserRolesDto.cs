using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class UserRolesDto
    {
        public IList<string> Roles { get; set; }
        public bool IsTrial { get; set; }
    }
}
