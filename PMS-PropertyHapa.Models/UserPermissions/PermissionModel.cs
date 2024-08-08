using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.UserPermissions
{
    public class PermissionModel
    {
        public int Id { get; set; }
        public int? EnumId { get; set; }
        public string Name { get; set; }
        public int PermissionCategoryId { get; set; }
        public string PermissionCategory { get; set; }
        public string RoleId { get; set; }
        public bool IsChecked { get; set; }
    }
}
