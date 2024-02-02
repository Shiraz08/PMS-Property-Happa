using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class TblRolePage : BaseEntities
    {
        public int Id { get; set; }

        public string RoleTitle { get; set; }

        public string RoleKey { get; set; }

        public bool? RoleActive { get; set; }
    }
}
