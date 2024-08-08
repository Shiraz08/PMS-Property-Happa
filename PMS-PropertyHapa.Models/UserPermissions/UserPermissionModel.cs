using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.UserPermissions
{
    public class UserPermissionModel
    {
        public string PermissionName { get; set; }
        public string PermissionCategory { get; set; }
        public int PermissionId { get; set; }
        public int PermissionCategoryId { get; set; }
        public List<Permissions> Permissions { get; set; }
        public int Id { get; set; }
        public int? EnumId { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsChecked { get; set; }
    }
    public class Permissions
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
