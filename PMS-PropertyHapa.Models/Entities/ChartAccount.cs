using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class ChartAccount : BaseEntities 
    {
        public int ChartAccountId { get; set; }
        public int AccountTypeId { get; set; }
        public int AccountSubTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsSubAccount { get; set; }
        public int? ParentAccountId { get; set; }
    }
}
