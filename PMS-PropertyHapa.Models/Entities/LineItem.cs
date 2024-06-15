using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class LineItem : BaseEntities
    {
        [Key]
        public int LineItemId { get; set; }
        public int TaskRequestId { get; set; }
        [ForeignKey("TaskRequestId")]
        public virtual TaskRequest TaskRequest { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int ChartAccountId { get; set; }
        [ForeignKey("ChartAccountId")]
        public virtual ChartAccount ChartAccount { get; set; }
        public string Memo { get; set; }
    }
}
