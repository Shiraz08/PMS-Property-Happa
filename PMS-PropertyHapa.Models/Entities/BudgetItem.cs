using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class BudgetItem : BaseEntities
    {
        [Key]
        public int BudgetItemId { get; set; }
        public string AccountName { get; set; }
        public string Period { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }
        public BudgetItemMonth? BudgetItemMonth { get; set; }
    }
}
