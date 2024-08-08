using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public int? EnumId { get; set; }
        public string Name { get; set; }
        public int PermissionCategoryId { get; set; }
    }
}
