using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class AssetsUnits : BaseEntities
    {
        [Key]
        public int UnitId { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public virtual Assets Asset { get; set; }
        public string UnitName { get; set; }
        public int Beds { get; set; }
        public int Bath { get; set; }
        public int Size { get; set; }
        public decimal Rent { get; set; }
    }
}
