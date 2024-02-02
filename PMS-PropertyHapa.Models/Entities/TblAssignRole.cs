using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class TblAssignRole : BaseEntities
    {
        public int Id { get; set; }

        public string RolePageId { get; set; }

        public string RoleTitle { get; set; }

        public bool? RoleActive { get; set; }

        public string GroupId { get; set; }

        public string GroupName { get; set; }
    }
}
