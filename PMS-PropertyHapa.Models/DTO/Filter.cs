using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class Filter
    {
        public string AddedBy { get; set; }
        public List<int?>? AssetIds { get; set; }
        public List<int?>? AccountTypeIds { get; set; }
    }
}
