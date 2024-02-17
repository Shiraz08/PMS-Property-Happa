using Microsoft.AspNetCore.Identity;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Migration.Data
{
    public class RoleEdit
    {
        public IdentityRole? Role { get; set; }
        public IEnumerable<ApplicationUser>? Members { get; set; }
        public IEnumerable<ApplicationUser>? NonMembers { get; set; }
    }
}
