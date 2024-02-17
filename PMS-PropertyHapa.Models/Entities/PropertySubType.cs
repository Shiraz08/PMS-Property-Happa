using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class PropertySubType : BaseEntities
    {
        [Key]
        public int PropertySubTypeId { get; set; }
        public int PropertyTypeId { get; set; }
        public string PropertySubTypeName { get; set; }
        public string Icon_String { get; set; }
        public string Icon_SVG { get; set; }
    }
}
