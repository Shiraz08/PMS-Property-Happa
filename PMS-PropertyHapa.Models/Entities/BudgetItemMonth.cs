using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class BudgetItemMonth : BaseEntities
    {
        [Key]
        public int BudgetItemMonthID { get; set; }
        public int BudgetItemId { get; set; }
        public BudgetItem BudgetItem { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Jan { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Feb { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? March { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? April { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? May { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? June { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? July { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Aug { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Sep { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Oct { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Nov { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Dec { get; set; }
        // for yearly 
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? YearStart { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? YearEnd { get; set; }

        // for quarterly 
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? quat1 { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? quat2 { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? quat4 { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? quat5 { get; set; }
    }
}
